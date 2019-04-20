using NetPrints.Core;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using NetPrints.Translator;
using System.Diagnostics;
using NetPrints.Serialization;
using NetPrints.Compilation;

namespace NetPrints.Core
{
    [Flags]
    public enum ProjectCompilationOutput
    {
        Nothing = 0,
        SourceCode = 1,
        Binaries = 2,
        Errors = 4,
        All = SourceCode | Binaries | Errors,
    }

    public enum BinaryType
    {
        SharedLibrary,
        Executable,
    }

    /// <summary>
    /// Project model.
    /// </summary>
    [DataContract]
    public class Project : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly IEnumerable<FrameworkAssemblyReference> DefaultReferences = new FrameworkAssemblyReference[]
        {
            new FrameworkAssemblyReference(".NETFramework/v4.5/System.dll"),
            new FrameworkAssemblyReference(".NETFramework/v4.5/System.Core.dll"),
            new FrameworkAssemblyReference(".NETFramework/v4.5/mscorlib.dll"),
        };

        private static readonly DataContractSerializer ProjectSerializer = new DataContractSerializer(typeof(Project));

        /// <summary>
        /// Classes contained in this project.
        /// </summary>
        public ObservableRangeCollection<ClassGraph> Classes
        {
            get;
            private set;
        } = new ObservableRangeCollection<ClassGraph>();

        /// <summary>
        /// Name of the project.
        /// </summary>
        [DataMember]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Version of the editor that the project was saved in.
        /// </summary>
        [DataMember]
        public Version SaveVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Path to the last successfully compiled assembly.
        /// </summary>
        [DataMember]
        public string LastCompiledAssemblyPath
        {
            get;
            set;
        }

        /// <summary>
        /// Path to the project file.
        /// </summary>
        public string Path
        {
            get;
            set;
        }

        /// <summary>
        /// Default namespace of newly created classes.
        /// </summary>
        [DataMember]
        public string DefaultNamespace
        {
            get;
            set;
        }

        /// <summary>
        /// Paths to files for the class models within this project.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<string> ClassPaths
        {
            get;
            set;
        } = new ObservableRangeCollection<string>();

        /// <summary>
        /// References of this project.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<CompilationReference> References
        {
            get;
            set;
        } = new ObservableRangeCollection<CompilationReference>();

        /// <summary>
        /// Determines what gets output during compilation.
        /// </summary>
        [DataMember]
        public ProjectCompilationOutput CompilationOutput
        {
            get;
            set;
        }

        /// <summary>
        /// Type of the binary that we want to output.
        /// </summary>
        [DataMember]
        public BinaryType OutputBinaryType
        {
            get;
            set;
        }

        private Project()
        {
        }

        public string GetClassStoragePath(ClassGraph cls)
        {
            return $"{cls.FullName}.netpc";
        }

        /// <summary>
        /// Saves the project to its path.
        /// </summary>
        public void Save()
        {
            // Save all classes
            foreach (ClassGraph cls in Classes)
            {
                SaveClassInProjectDirectory(cls);
            }

            // Set class paths from class storage paths
            ClassPaths = new ObservableRangeCollection<string>(Classes.Select(c => GetClassStoragePath(c)));

            SaveVersion = Assembly.GetExecutingAssembly().GetName().Version;

            using FileStream fileStream = File.Open(Path, FileMode.Create);
            ProjectSerializer.WriteObject(fileStream, this);
        }

        /// <summary>
        /// Creates a new project.
        /// </summary>
        /// <param name="name">Name of the project.</param>
        /// <param name="defaultNamespace">Default namespace of the project.</param>
        /// <param name="addDefaultReferences">Whether to add default references to the project.</param>
        /// <returns>The created project.</returns>
        public static Project CreateNew(string name, string defaultNamespace, bool addDefaultReferences=true,
            ProjectCompilationOutput compilationOutput=ProjectCompilationOutput.All)
        {
            Project project = new Project()
            {
                Name = name,
                DefaultNamespace = defaultNamespace,
                CompilationOutput = compilationOutput
            };

            if (addDefaultReferences)
            {
                project.References.AddRange(DefaultReferences);
            }

            return project;
        }

        /// <summary>
        /// Loads a project from a path.
        /// </summary>
        /// <param name="path">Path to the project file.</param>
        /// <returns>Loaded project or null if unsuccessful</returns>
        public static Project LoadFromPath(string path)
        {
            using FileStream fileStream = File.OpenRead(path);

            if (ProjectSerializer.ReadObject(fileStream) is Project project)
            {
                project.Path = path;

                // Load classes
                ConcurrentBag<ClassGraph> classes = new ConcurrentBag<ClassGraph>();

                Parallel.ForEach(project.ClassPaths, classPath =>
                {
                    ClassGraph cls = SerializationHelper.LoadClass(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(project.Path), classPath));
                    cls.Project = project;
                    classes.Add(cls);
                });

                project.Classes.ReplaceRange(classes.OrderBy(c => c.Name));

                return project;
            }

            return null;
        }

        public bool CanCompileAndRun
        {
            get => CanCompile && OutputBinaryType == BinaryType.Executable
                && CompilationOutput.HasFlag(ProjectCompilationOutput.Binaries);
        }

        public bool CanCompile
        {
            get => !isCompiling;
        }

        public string CompilationMessage
        {
            get => compilationMessage;
            set
            {
                if (compilationMessage != value)
                {
                    compilationMessage = value;
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
                }
            }
        }

        private bool isCompiling;

        public bool LastCompilationSucceeded
        {
            get => lastCompilationSucceeded;
            set
            {
                if (lastCompilationSucceeded != value)
                {
                    lastCompilationSucceeded = value;
                }
            }
        }

        private bool lastCompilationSucceeded = false;

        public ObservableRangeCollection<string> LastCompileErrors
        {
            get => lastCompileErrors;
            set
            {
                if (lastCompileErrors != value)
                {
                    lastCompileErrors = value;
                }
            }
        }

        private ObservableRangeCollection<string> lastCompileErrors;

        public async void CompileProject()
        {
            // Check if we are already compiling
            if (!CanCompile || CompilationOutput == ProjectCompilationOutput.Nothing)
            {
                return;
            }

            IsCompiling = true;
            CompilationMessage = "Compiling...";

            var references = References.ToArray();

            // Compile in another thread
            var results = await Task.Run(() =>
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

                ConcurrentBag<string> classSources = new ConcurrentBag<string>();

                // Translate classes in parallel
                Parallel.ForEach(Classes, cls =>
                {
                    // Translate the class to C#
                    ClassTranslator classTranslator = new ClassTranslator();

                    string code = classTranslator.TranslateClass(cls);

                    string[] directories = cls.FullName.Split('.');
                    directories = directories
                        .Take(directories.Length - 1)
                        .Prepend(compiledDir)
                        .ToArray();

                    // Write source to file
                    string outputDirectory = System.IO.Path.Combine(directories);

                    System.IO.Directory.CreateDirectory(outputDirectory);

                    if (CompilationOutput.HasFlag(ProjectCompilationOutput.SourceCode))
                    {
                        File.WriteAllText(System.IO.Path.Combine(outputDirectory, $"{cls.Name}.cs"), code);
                    }

                    classSources.Add(code);
                });

                bool generateExecutable = OutputBinaryType == BinaryType.Executable;
                string ext = generateExecutable ? "exe" : "dll";

                string outputPath = System.IO.Path.Combine(compiledDir, $"{Name}.{ext}");

                // Create compiler on other app domain, compile, unload the app domain

                var codeCompiler = new Compilation.CodeCompiler();

                bool deleteBinaries = !CompilationOutput.HasFlag(ProjectCompilationOutput.Binaries) && !File.Exists(outputPath);

                var assemblyPaths = references.OfType<AssemblyReference>().Select(a => a.AssemblyPath);

                var sources = classSources
                    .Concat(references
                        .OfType<SourceDirectoryReference>()
                        .Where(sourceRef => sourceRef.IncludeInCompilation)
                        .SelectMany(sourceRef => sourceRef.SourceFilePaths)
                        .Select(sourcePath => File.ReadAllText(sourcePath)))
                    .Distinct()
                    .ToArray();

                CodeCompileResults compilationResults = codeCompiler.CompileSources(
                    outputPath, assemblyPaths, sources, generateExecutable);

                // Delete the output binary if we don't want it.
                // TODO: Don't generate it in the first place.
                if (compilationResults.PathToAssembly != null && deleteBinaries)
                {
                    if (File.Exists(compilationResults.PathToAssembly))
                    {
                        File.Delete(compilationResults.PathToAssembly);
                    }
                }

                // Write errors to file
                if (CompilationOutput.HasFlag(ProjectCompilationOutput.Errors))
                {
                    File.WriteAllText(System.IO.Path.Combine(compiledDir, $"{Name}_errors.txt"),
                        string.Join(Environment.NewLine, compilationResults.Errors));
                }

                return compilationResults;
            });

            LastCompilationSucceeded = results.Success;
            LastCompileErrors = new ObservableRangeCollection<string>(results.Errors);

            if (LastCompilationSucceeded)
            {
                LastCompiledAssemblyPath = results.PathToAssembly;
                CompilationMessage = "Build succeeded";
            }
            else
            {
                CompilationMessage = $"Build failed with {LastCompileErrors.Count} error(s)";
            }

            IsCompiling = false;
        }

        public IEnumerable<string> GenerateClassSources()
        {
            if (Classes is null)
            {
                return new string[0];
            }

            ConcurrentBag<string> classSources = new ConcurrentBag<string>();

            // Translate classes in parallel
            Parallel.ForEach(Classes, cls =>
            {
                // Translate the class to C#
                ClassTranslator classTranslator = new ClassTranslator();

                string code = classTranslator.TranslateClass(cls);

                classSources.Add(code);
            });

            return classSources;
        }

        public void RunProject()
        {
            if (OutputBinaryType != BinaryType.Executable || !CompilationOutput.HasFlag(ProjectCompilationOutput.Binaries))
            {
                throw new InvalidOperationException("Can only run executable projects which output their binaries.");
            }

            string projectDir = System.IO.Path.GetDirectoryName(Path);
            string exePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(projectDir, $"Compiled_{Name}", $"{Name}.exe"));

            if (!File.Exists(exePath))
            {
                throw new Exception($"The executable does not exist at {exePath}");
            }

            Process.Start(exePath);
        }

        private void FixReferencePaths()
        {
            var referencesToRemove = new List<CompilationReference>();

            // Fix references
            foreach (var reference in References)
            {
                if (reference is AssemblyReference assemblyReference)
                {
                    // Check if the assembly exists at the path and
                    // give the user a chance to select another one.
                    if (!File.Exists(assemblyReference.AssemblyPath))
                    {
                        throw new NotImplementedException();

                        // TODO: Fix
                        /*var openFileDialog = new OpenFileDialog()
                        {
                            Title = $"Open replacement for {assemblyReference}",
                            CheckFileExists = true,
                        };

                        if (openFileDialog.ShowDialog() == true)
                        {
                            assemblyReference.AssemblyPath = openFileDialog.FileName;
                        }
                        else
                        {
                            referencesToRemove.Add(reference);
                        }*/
                    }
                }
            }

            // Remove references which couldn't be fixed
            if (referencesToRemove.Count > 0)
            {
                References.RemoveRange(referencesToRemove);

                // TODO
                /*MessageBox.Show("The following assemblies could not be found and have been removed from the project:\n\n" +
                    string.Join(Environment.NewLine, referencesToRemove.Select(n => n.ToString())),
                    "Could not load some assemblies", MessageBoxButton.OK, MessageBoxImage.Warning);*/
            }
        }

        #region Create / Load / Save Project
        /// <summary>
        /// Saves the given class in the project directory.
        /// </summary>
        /// <param name="cls">Class to save.</param>
        public void SaveClassInProjectDirectory(ClassGraph cls)
        {
            string outputPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path), GetClassStoragePath(cls));

            // Save in same directory as project
            SerializationHelper.SaveClass(cls, outputPath);
        }

        #endregion

        #region Creating and loading classes
        public ClassGraph CreateNewClass()
        {
            // Make a class name that isn't already a file and isn't
            // already a class in the project.

            IList<string> existingFiles = System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(Path))
                .Select(f => System.IO.Path.GetFileNameWithoutExtension(f))
                .Concat(Classes.Select(c => System.IO.Path.GetFileNameWithoutExtension(GetClassStoragePath(c))))
                .ToList();

            string storageName = $"{DefaultNamespace}.MyClass";
            storageName = NetPrintsUtil.GetUniqueName(storageName, existingFiles);

            // TODO: Might break if GetUniqueName adds a dot
            // (which it doesn't at the time of writing, it just adds
            // numbers, but this is not guaranteed forever).
            string name = storageName.Split('.').Last();

            ClassGraph cls = new ClassGraph()
            {
                Name = name,
                Namespace = DefaultNamespace,
                Project = this,
            };

            // TODO: SaveClassInProjectDirectory(clsVM);
            Classes.Add(cls);

            return cls;
        }

        public ClassGraph AddExistingClass(string path)
        {
            // Check if a class with the same storage name is already loaded
            string fileName = System.IO.Path.GetFileName(path);
            ClassGraph cls = Classes.FirstOrDefault(c => string.Equals(GetClassStoragePath(c), fileName, StringComparison.OrdinalIgnoreCase));

            bool loadAndSave = false;

            if (cls != null)
            {
                // Ask if we should overwrite if it already exists
                // TODO: Probably want to move this into a view instead of here in
                // the viewmodel.
                /*MessageBoxResult result = MessageBox.Show($"File with name {fileName} already exists in this project. Overwrite it?", "File already exists",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                // Overwrite the class if chosen
                if (result == MessageBoxResult.Yes)
                {
                    // Remove the old class and load the new one
                    Project.Classes.Remove(cls);
                    loadAndSave = true;
                }*/
            }
            else
            {
                // Load the new class
                loadAndSave = true;
            }

            if (loadAndSave)
            {
                // Load the class and save it relative to the project
                cls = SerializationHelper.LoadClass(path);
                cls.Project = this;
                SaveClassInProjectDirectory(cls);
                Classes.Add(cls);
            }

            return cls;
        }
        #endregion

        [OnDeserialized]
        private void FixDefaults(StreamingContext context)
        {
            Classes = new ObservableRangeCollection<ClassGraph>();
        }
    }
}
