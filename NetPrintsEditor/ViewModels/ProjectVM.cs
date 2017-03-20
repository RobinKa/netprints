using NetPrints.Core;
using NetPrints.Extensions;
using NetPrints.Serialization;
using NetPrintsEditor.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetPrintsEditor.ViewModels
{
    public class ProjectVM : INotifyPropertyChanged
    {
        public bool IsProjectOpen
        {
            get => project != null;
        }

        public ObservableCollection<ClassVM> Classes
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

        private ObservableCollection<ClassVM> classes;

        public ObservableCollection<string> ClassPaths
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
                    project = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsProjectOpen));
                }
            }
        }

        private Project project;
        
        public ProjectVM(Project project)
        {
            Project = project;
        }

        public static ProjectVM CreateNew(string name, string defaultNamespace, bool addDefaultAssemblies = true)
        {
            ProjectVM p = new ProjectVM(Project.CreateNew(name, defaultNamespace, addDefaultAssemblies))
            {
                Classes = new ObservableCollection<ClassVM>()
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
                classes.Add(new ClassVM(cls));
            });

            p.Classes = new ObservableCollection<ClassVM>(classes.OrderBy(c => c.Name));

            return p;
        }

        public void Save()
        {
            // Set class paths from class storage paths
            ClassPaths = new ObservableCollection<string>(Classes.Select(c => c.StoragePath));
            Project.Save();
        }

        #region Creating and loading classes
        public ClassVM CreateNewClass()
        {
            Class cls = new Class()
            {
                // TODO: Make name unique together with namespace instead of name alone
                Name = NetPrintsUtil.GetUniqueName("MyClass", Classes.Select(c => c.Name).ToList()),
                Namespace = DefaultNamespace
            };

            ClassVM clsVM = new ClassVM(cls);

            Classes.Add(clsVM);

            return clsVM;
        }

        public ClassVM AddExistingClass(string path)
        {
            if(!Classes.Any(c => System.IO.Path.GetFullPath(c.StoragePath) == System.IO.Path.GetFullPath(path)))
            {
                ClassVM cls = new ClassVM(SerializationHelper.LoadClass(path));
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
