using Microsoft.Win32;
using NetPrints.Core;
using NetPrints.Serialization;
using NetPrints.Translator;
using NetPrintsEditor.Compilation;
using NetPrintsEditor.Models;
using NetPrintsEditor.Reflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NetPrintsEditor.ViewModels
{
    public class ProjectVM : INotifyPropertyChanged
    {
#region Singleton
        public static ProjectVM Instance
        {
            get => instance;
        }

        private static ProjectVM instance;
#endregion

        public bool IsProjectOpen
        {
            get => project != null;
        }

        public ObservableRangeCollection<ClassVM> Classes
        {
            get => classes;
            set
            {
                if(classes != value)
                {
                    classes = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableRangeCollection<ClassVM> classes;

        public ObservableRangeCollection<string> ClassPaths
        {
            get => project.ClassPaths;
            private set
            {
                if(project.ClassPaths != value)
                {
                    project.ClassPaths = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LastCompiledAssemblyPath
        {
            get => project.LastCompiledAssemblyPath;
            set
            {
                if(project.LastCompiledAssemblyPath != value)
                {
                    project.LastCompiledAssemblyPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableRangeCollection<string> LastCompileErrors
        {
            get => lastCompileErrors;
            set
            {
                if(lastCompileErrors != value)
                {
                    lastCompileErrors = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableRangeCollection<string> lastCompileErrors;

        public ObservableRangeCollection<LocalAssemblyName> Assemblies
        {
            get => project.Assemblies;
            set
            {
                if(project.Assemblies != value)
                {
                    if(project.Assemblies != null)
                    {
                        project.Assemblies.CollectionChanged -= OnAssembliesChanged;
                    }

                    project.Assemblies = value;

                    if (project.Assemblies != null)
                    {
                        value.CollectionChanged += OnAssembliesChanged;
                        FixAssemblyPaths();
                    }

                    ReloadReflectionProvider();
                    OnPropertyChanged();
                }
            }
        }

        private void OnAssembliesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FixAssemblyPaths();
            ReloadReflectionProvider();
        }

        public string Path
        {
            get => project.Path;
            set
            {
                if (project.Path != value)
                {
                    project.Path = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DefaultNamespace
        {
            get => project.DefaultNamespace;
            set
            {
                if (project.DefaultNamespace != value)
                {
                    project.DefaultNamespace = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => project != null ? project.Name : "No Project Loaded";
            set
            {
                if (project.Name != value)
                {
                    project.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public ProjectCompilationOutput CompilationOutput
        {
            get => project != null ? project.CompilationOutput : ProjectCompilationOutput.Nothing;
            set
            {
                if (project.CompilationOutput != value)
                {
                    project.CompilationOutput = value;
                    OnPropertyChanged();
                }
            }
        }

        public Project Project
        {
            get => project;
            set
            {
                if (project != value)
                {
                    if(project != null)
                    {
                        Assemblies.CollectionChanged -= OnAssembliesChanged;
                    }

                    project = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsProjectOpen));
                    OnPropertyChanged(nameof(CanCompile));
                    
                    if (project != null)
                    {
                        Assemblies.CollectionChanged += OnAssembliesChanged;
                        FixAssemblyPaths();
                    }

                    ReloadReflectionProvider();
                }
            }
        }

        public bool CanCompile
        {
            get => !isCompiling && IsProjectOpen;
        }

        public string CompilationMessage
        {
            get => compilationMessage;
            set
            {
                if(compilationMessage != value)
                {
                    compilationMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        private string compilationMessage = "Ready";

        public bool IsCompiling
        {
            get => isCompiling;
            set
            {
                if (isCompiling != value)
                {
                    isCompiling = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanCompile));
                }
            }
        }

        public bool LastCompilationSucceeded
        {
            get => lastCompilationSucceeded;
            set
            {
                if(lastCompilationSucceeded != value)
                {
                    lastCompilationSucceeded = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool lastCompilationSucceeded = false;

        private bool isCompiling = false;

        private Project project;

        public IReflectionProvider ReflectionProvider { get; private set; }

        public ObservableRangeCollection<TypeSpecifier> NonStaticTypes
        {
            get;
        } = new ObservableRangeCollection<TypeSpecifier>();

        // Keep track of the save location of classes, so if they change their
        // name we can delete the old file on saving.
        private Dictionary<Class, string> previousStoragePath = new Dictionary<Class, string>();

        public ProjectVM(Project project)
        {
            Project = project;
            instance = this;
        }

        public void CompileProject(bool generateExecutable)
        {
            // Check if we are already compiling
            if(!CanCompile || project.CompilationOutput == ProjectCompilationOutput.Nothing)
            {
                return;
            }

            IsCompiling = true;
            CompilationMessage = "Compiling...";

            // Unload the app domain of the previous assembly
            // TODO: Remove this as it is not relevant anymore since
            // the .NET Core transition.
            if (ReflectionProvider != null)
            {
                ReflectionProvider = null;
                GC.Collect();
            }

            // Save original thread dispatcher
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

            // Compile in another thread
            new Thread(() =>
            {
                string projectDir = System.IO.Path.GetDirectoryName(Path);
                string compiledDir = System.IO.Path.Combine(projectDir, $"Compiled_{Name}");

                
                DirectoryInfo compiledDirInfo = new DirectoryInfo(compiledDir);
                if (compiledDirInfo.Exists)
                {
                    // Delete existing compiled output
                    foreach (FileInfo file in compiledDirInfo.EnumerateFiles())
                    {
                        file.Delete();
                    }

                    foreach (DirectoryInfo dir in compiledDirInfo.EnumerateDirectories())
                    {
                        dir.Delete(true);
                    }
                }
                else
                {
                    Directory.CreateDirectory(compiledDir);
                }

                ConcurrentBag<string> sources = new ConcurrentBag<string>();

                // Translate classes in parallel
                Parallel.ForEach(Classes, cls =>
                {
                    // Translate the class to C#
                    ClassTranslator classTranslator = new ClassTranslator();

                    string code = classTranslator.TranslateClass(cls.Class);

                    string[] directories = cls.FullName.Split(".");
                    directories = directories
                        .Take(directories.Count() - 1)
                        .Prepend(compiledDir)
                        .ToArray();

                    // Write source to file
                    string outputDirectory = System.IO.Path.Combine(directories);

                    System.IO.Directory.CreateDirectory(outputDirectory);

                    if (CompilationOutput.HasFlag(ProjectCompilationOutput.SourceCode))
                    {
                        File.WriteAllText(System.IO.Path.Combine(outputDirectory, $"{cls.Name}.cs"), code);
                    }

                    sources.Add(code);
                });

                string ext = generateExecutable ? "exe" : "dll";

                string outputPath = System.IO.Path.Combine(compiledDir, $"{Project.Name}.{ext}");

                // Create compiler on other app domain, compile, unload the app domain

                var codeCompiler = new Compilation.CodeCompiler();

                bool deleteBinaries = !CompilationOutput.HasFlag(ProjectCompilationOutput.Binaries) && !File.Exists(outputPath);

                CodeCompileResults results = codeCompiler.CompileSources(
                    outputPath, Assemblies.Select(a => a.Path).ToArray(), sources, generateExecutable);

                // Delete the output binary if we don't want it.
                // TODO: Don't generate it in the first place.
                if (results.PathToAssembly != null && deleteBinaries)
                {
                    if (File.Exists(results.PathToAssembly))
                    {
                        File.Delete(results.PathToAssembly);
                    }
                }

                // Write errors to file
                if (CompilationOutput.HasFlag(ProjectCompilationOutput.Errors))
                {
                    File.WriteAllText(System.IO.Path.Combine(compiledDir, $"{Project.Name}_errors.txt"),
                        string.Join(Environment.NewLine, results.Errors));
                }
                
                // Notify UI that we are done and refresh reflection provider

                dispatcher.Invoke(() =>
                {
                    LastCompilationSucceeded = results.Success;
                    LastCompileErrors = new ObservableRangeCollection<string>(results.Errors);

                    if (LastCompilationSucceeded)
                    {
                        LastCompiledAssemblyPath = results.PathToAssembly;
                        CompilationMessage = $"Build succeeded";
                    }
                    else
                    {
                        CompilationMessage = $"Build failed with {LastCompileErrors.Count} error(s)";
                    }

                    ReloadReflectionProvider();

                    IsCompiling = false;
                });
            }).Start();
        }

        private void ReloadReflectionProvider()
        {
            // Load newly compiled assembly and referenced assemblies
            List<string> assembliesToReflectOn = Assemblies.Select(a => a.Path).ToList();
            if(!string.IsNullOrEmpty(LastCompiledAssemblyPath) && File.Exists(LastCompiledAssemblyPath))
            {
                assembliesToReflectOn.Add(LastCompiledAssemblyPath);
            }

            ReflectionProvider = new MemoizedReflectionProvider(new ReflectionProvider(assembliesToReflectOn));

            NonStaticTypes.ReplaceRange(ReflectionProvider.GetNonStaticTypes());
        }

        public void RunProject()
        {
            string projectDir = System.IO.Path.GetDirectoryName(Path);
            string exePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(projectDir, $"Compiled_{Name}", $"{Project.Name}.exe"));

            if(!File.Exists(exePath))
            {
                throw new Exception($"The executable does not exist at {exePath}");
            }

            Process.Start(exePath);
        }

        private void FixAssemblyPaths()
        {
            List<LocalAssemblyName> assembliesToRemove = new List<LocalAssemblyName>();

            // Fix assembly references
            foreach (LocalAssemblyName localAssemblyName in Assemblies)
            {
                if (!localAssemblyName.FixPath())
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog()
                    {
                        Title = $"Open {localAssemblyName.Name}"
                    };

                    if (openFileDialog.ShowDialog() == true)
                    {
                        localAssemblyName.Path = openFileDialog.FileName;
                        if (!localAssemblyName.FixPath())
                        {
                            assembliesToRemove.Add(localAssemblyName);
                        }
                    }
                    else
                    {
                        assembliesToRemove.Add(localAssemblyName);
                    }
                }
            }

            // Remove assemblies references which couldnt be fixed
            if (assembliesToRemove.Count > 0)
            {
                Assemblies.RemoveRange(assembliesToRemove);

                MessageBox.Show("The following assemblies could not be found and have been removed from the project:\n\n" +
                    string.Join(Environment.NewLine, assembliesToRemove.Select(n => n.Name)),
                    "Could not load some assemblies", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #region Create / Load / Save Project
        public static ProjectVM CreateNew(string name, string defaultNamespace, bool addDefaultAssemblies = true)
        {
            ProjectVM p = new ProjectVM(Project.CreateNew(name, defaultNamespace, addDefaultAssemblies))
            {
                Classes = new ObservableRangeCollection<ClassVM>()
            };

            return p;
        }

        public static ProjectVM LoadFromPath(string path)
        {
            ProjectVM p =  new ProjectVM(Project.LoadFromPath(path));

            ConcurrentBag<ClassVM> classes = new ConcurrentBag<ClassVM>();

            Parallel.ForEach(p.ClassPaths, classPath =>
            {
                Class cls = SerializationHelper.LoadClass(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(p.Path), classPath));
                classes.Add(new ClassVM(cls) { Project = p });
            });

            p.Classes = new ObservableRangeCollection<ClassVM>(classes.OrderBy(c => c.Name));

            foreach (ClassVM cls in p.classes)
            {
                p.previousStoragePath[cls.Class] = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(p.Path), cls.StoragePath);
            }

            return p;
        }

        /// <summary>
        /// Saves the given class in the project directory.
        /// </summary>
        /// <param name="cls">Class to save.</param>
        public void SaveClassInProjectDirectory(ClassVM cls)
        {
            string outputPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path), cls.StoragePath);

            // Delete old save file if different path and exists
            if (previousStoragePath.TryGetValue(cls.Class, out string prevPath) &&
                !string.Equals(
                    System.IO.Path.GetFullPath(prevPath),
                    System.IO.Path.GetFullPath(outputPath),
                    StringComparison.OrdinalIgnoreCase) &&
                File.Exists(prevPath))
            {
                File.Delete(prevPath);
            }

            // Save in same directory as project
            SerializationHelper.SaveClass(cls.Class, outputPath);
            previousStoragePath[cls.Class] = outputPath;
        }

        /// <summary>
        /// Saves the project and all its classes into the project directory.
        /// </summary>
        public void Save()
        {
            // Save all classes
            foreach (ClassVM cls in Classes)
            {
                SaveClassInProjectDirectory(cls);
            }

            // Set class paths from class storage paths
            ClassPaths = new ObservableRangeCollection<string>(Classes.Select(c => c.StoragePath));

            // Save project file
            Project.Save();
        }
        #endregion

        #region Creating and loading classes
        public ClassVM CreateNewClass()
        {
            // Make a class name that isn't already a file and isn't
            // already a class in the project.

            IList<string> existingFiles = System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(Path))
                .Select(f => System.IO.Path.GetFileNameWithoutExtension(f))
                .Concat(Classes.Select(c => System.IO.Path.GetFileNameWithoutExtension(c.StoragePath)))
                .ToList();

            string storageName = $"{DefaultNamespace}.MyClass";
            storageName = NetPrintsUtil.GetUniqueName(storageName, existingFiles);

            // TODO: Might break if GetUniqueName adds a dot
            // (which it doesn't at the time of writing, it just adds
            // numbers, but this is not guaranteed forever).
            string name = storageName.Split(".").Last();

            Class cls = new Class()
            {
                Name = name,
                Namespace = DefaultNamespace
            };

            ClassVM clsVM = new ClassVM(cls) { Project = this };

            SaveClassInProjectDirectory(clsVM);
            Classes.Add(clsVM);

            return clsVM;
        }

        public ClassVM AddExistingClass(string path)
        {
            // Check if a class with the same storage name is already loaded
            string fileName = System.IO.Path.GetFileName(path);
            ClassVM cls = Classes.FirstOrDefault(c => string.Equals(c.StoragePath, fileName, StringComparison.OrdinalIgnoreCase));

            bool loadAndSave = false;

            if (cls != null)
            {
                // Ask if we should overwrite if it already exists
                // TODO: Probably want to move this into a view instead of here in
                // the viewmodel.
                MessageBoxResult result = MessageBox.Show($"File with name {fileName} already exists in this project. Overwrite it?", "File already exists",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                // Overwrite the class if chosen
                if (result == MessageBoxResult.Yes)
                {
                    // Remove the old class and load the new one
                    Classes.Remove(cls);
                    loadAndSave = true;
                }
            }
            else
            {
                // Load the new class
                loadAndSave = true;
            }

            if (loadAndSave)
            {
                // Load the class and save it relative to the project
                cls = new ClassVM(SerializationHelper.LoadClass(path)) { Project = this };
                SaveClassInProjectDirectory(cls);
                Classes.Add(cls);
            }

            return cls;
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
