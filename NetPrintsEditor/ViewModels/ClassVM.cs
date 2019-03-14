using NetPrints.Core;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NetPrintsEditor.ViewModels
{
    public class ClassVM : INotifyPropertyChanged
    {
        public ProjectVM Project
        {
            get => project;
            set
            {
                if(project != value)
                {
                    project = value;
                    OnPropertyChanged();
                }
            }
        }

        private ProjectVM project;

        // Wrapped attributes of Class
        public ObservableViewModelCollection<VariableVM, Variable> Attributes
        {
            get => attributes;
            set
            {
                if(attributes != value)
                {
                    attributes = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableViewModelCollection<VariableVM, Variable> attributes;

        public ObservableViewModelCollection<MethodVM, Method> Methods
        {
            get => methods;
            set
            {
                if(methods != value)
                {
                    methods = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableViewModelCollection<MethodVM, Method> methods;

        public TypeSpecifier SuperType
        {
            get => cls.SuperType;
            set
            {
                cls.SuperType = value;
                OnPropertyChanged();
            }
        }

        public TypeSpecifier Type
        {
            get => cls.Type;
        }

        public string FullName
        {
            get => Type.Name;
        }

        public string Namespace
        {
            get => cls.Namespace;
            set
            {
                cls.Namespace = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StoragePath));
                OnPropertyChanged(nameof(FullName));
                OnPropertyChanged(nameof(Type));
            }
        }

        public string Name
        {
            get => cls.Name;
            set
            {
                cls.Name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StoragePath));
                OnPropertyChanged(nameof(FullName));
                OnPropertyChanged(nameof(Type));
            }
        }

        public ClassModifiers Modifiers
        {
            get => cls.Modifiers;
            set
            {
                cls.Modifiers = value;
                OnPropertyChanged();
            }
        }

        public Class Class
        {
            get => cls;
            set
            {
                if (cls != value)
                {
                    cls = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StoragePath
        {
            get => $"{Namespace}.{Name}.netpc";
        }

        private Class cls;

        public ClassVM(Class cls)
        {
            Class = cls;
        }

#region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if(propertyName == nameof(Class))
            {
                Methods = new ObservableViewModelCollection<MethodVM, Method>(
                    cls.Methods, m => new MethodVM(m) { Class = this } );

                Attributes = new ObservableViewModelCollection<VariableVM, Variable>(
                    cls.Attributes, a => new VariableVM(a));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
#endregion
    }
}
