using Microsoft.Win32;
using NetPrints.Core;
using NetPrints.Serialization;
using NetPrints.Translator;
using NetPrintsEditor.Compilation;
using NetPrintsEditor.Models;
using NetPrintsEditor.Reflection;
using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public ReflectionProvider ReflectionProvider
        {
            get => reflectionProviderWrapper;
        }

        public ObservableRangeCollection<TypeSpecifier> NonStaticTypes
        {
            get;
        } = new ObservableRangeCollection<TypeSpecifier>();

        private ReflectionProvider reflectionProviderWrapper;

        public ProjectVM(Project project)
        {
            Project = project;
            instance = this;
        }

        public void CompileProject(bool generateExecutable)
        {
            // Check if we are already compiling
            if(!CanCompile)
            {
                return;
            }

            IsCompiling = true;
            CompilationMessage = "Compiling...";

            //  Unload the app domain of the previous assembly
            if (reflectionProviderWrapper != null)
            {
                reflectionProviderWrapper = null;
                GC.Collect();
            }

            // Save original thread dispatcher
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

            // Compile in another thread
            new Thread(() =>
            {
                if (!Directory.Exists("Compiled"))
                {
                    Directory.CreateDirectory("Compiled");
                }

                ConcurrentBag<string> sources = new ConcurrentBag<string>();

                // Translate classes in parallel
                Parallel.ForEach(Classes, cls =>
                {
                    // Translate the class to C#
                    ClassTranslator classTranslator = new ClassTranslator();
                    string fullClassName = $"{cls.Namespace}.{cls.Name}";
                    string code = classTranslator.TranslateClass(cls.Class);

                    // Write source to file for examination
                    File.WriteAllText($"Compiled/{fullClassName}.txt", code);

                    sources.Add(code);
                });

                string ext = generateExecutable ? "exe" : "dll";

                string outputPath = $"Compiled/{Project.Name}.{ext}";

                // Create compiler on other app domain, compile, unload the app domain

                var codeCompiler = new Compilation.CodeCompiler();
                
                CodeCompileResults results = codeCompiler.CompileSources(
                    outputPath, Assemblies.Select(a => a.Path).ToArray(), sources, generateExecutable);

                // Write errors to file

                File.WriteAllText($"Compiled/{Project.Name}_errors.txt", 
                    string.Join(Environment.NewLine, results.Errors));
                
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

            reflectionProviderWrapper = new ReflectionProvider(assembliesToReflectOn);

            NonStaticTypes.ReplaceRange(ReflectionProvider.GetNonStaticTypes());
        }

        public void RunProject()
        {
            string exePath = System.IO.Path.GetFullPath($"Compiled/{Project.Name}.exe");

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
                Class cls = SerializationHelper.LoadClass(classPath);
                classes.Add(new ClassVM(cls) { Project = p });
            });

            p.Classes = new ObservableRangeCollection<ClassVM>(classes.OrderBy(c => c.Name));

            return p;
        }

        public void Save()
        {
            // Save all classes
            foreach (ClassVM cls in Classes)
            {
                SerializationHelper.SaveClass(cls.Class, cls.StoragePath);
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
            Class cls = new Class()
            {
                // TODO: Make name unique together with namespace instead of name alone
                Name = NetPrintsUtil.GetUniqueName("MyClass", Classes.Select(c => c.Name).ToList()),
                Namespace = DefaultNamespace
            };

            ClassVM clsVM = new ClassVM(cls) { Project = this };

            Classes.Add(clsVM);

            return clsVM;
        }

        public ClassVM AddExistingClass(string path)
        {
            if(!Classes.Any(c => System.IO.Path.GetFullPath(c.StoragePath) == System.IO.Path.GetFullPath(path)))
            {
                ClassVM cls = new ClassVM(SerializationHelper.LoadClass(path)) { Project = this };
                Classes.Add(cls);
                return cls;
            }
            else
            {
                return Classes.First(c => System.IO.Path.GetFullPath(c.StoragePath) == System.IO.Path.GetFullPath(path));
            }
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
