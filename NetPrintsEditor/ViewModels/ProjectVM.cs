using NetPrints.Core;
using NetPrints.Serialization;
using NetPrints.Translator;
using NetPrintsEditor.Models;
using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NetPrintsEditor.ViewModels
{
    public class ProjectVM : INotifyPropertyChanged
    {
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

        public ObservableRangeCollection<string> Assemblies
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

                    LoadedAssemblies.Clear();

                    if (project.Assemblies != null)
                    {
                        value.CollectionChanged += OnAssembliesChanged;
                        LoadedAssemblies.AddRange(ReflectionUtil.LoadAssemblies(project.Assemblies));
                    }

                    OnPropertyChanged();
                }
            }
        }

        private void OnAssembliesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    LoadedAssemblies.AddRange(ReflectionUtil.LoadAssemblies(e.NewItems.Cast<string>()));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    LoadedAssemblies.RemoveRange(ReflectionUtil.LoadAssemblies(e.OldItems.Cast<string>()));
                    break;

                case NotifyCollectionChangedAction.Replace:
                    LoadedAssemblies.RemoveRange(ReflectionUtil.LoadAssemblies(e.OldItems.Cast<string>()));
                    LoadedAssemblies.AddRange(ReflectionUtil.LoadAssemblies(e.NewItems.Cast<string>()));
                    break;

                case NotifyCollectionChangedAction.Reset:
                    LoadedAssemblies.Clear();
                    break;
            }

            ReflectionUtil.UpdateNonStaticTypes(LoadedAssemblies);
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

                    LoadedAssemblies?.Clear();

                    if(project != null)
                    {
                        Assemblies.CollectionChanged += OnAssembliesChanged;
                        LoadedAssemblies.AddRange(ReflectionUtil.LoadAssemblies(project.Assemblies));
                    }
                }
            }
        }

        public bool CanCompile
        {
            get => !isCompiling && IsProjectOpen;
        }

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
            get;
            set;
        } = false;

        private bool isCompiling = false;

        private Project project;

        public ObservableRangeCollection<Assembly> LoadedAssemblies
        {
            get;
            private set;
        } = new ObservableRangeCollection<Assembly>();
        
        public ProjectVM(Project project)
        {
            LoadedAssemblies.CollectionChanged += OnLoadedAssembliesChanged;

            Project = project;
        }

        private void OnLoadedAssembliesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ReflectionUtil.UpdateNonStaticTypes(LoadedAssemblies);
        }

        public void CompileProject(bool generateExecutable)
        {
            // Check if we are already compiling
            if(!CanCompile)
            {
                return;
            }

            IsCompiling = true;

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

                CompilerResults results = CompilerUtil.CompileSources(
                    outputPath, LoadedAssemblies, sources, generateExecutable);

                // Write errors to file
                File.WriteAllText($"Compiled/{Project.Name}_errors.txt", 
                    string.Join(Environment.NewLine, results.Errors.Cast<CompilerError>()));

                dispatcher.Invoke(() =>
                {
                    LastCompilationSucceeded = !results.Errors.HasErrors;
                    IsCompiling = false;
                });
            }).Start();
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
