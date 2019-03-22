using NetPrints.Core;
using NetPrints.Translator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

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

        /// <summary>
        /// Specifiers for methods that this class can override.
        /// </summary>
        public IEnumerable<MethodSpecifier> OverridableMethods
        {
            get => ProjectVM.Instance.ReflectionProvider.GetOverridableMethodsForType(SuperType);
        }

        public TypeSpecifier SuperType
        {
            get => cls?.SuperType;
            set
            {
                cls.SuperType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(OverridableMethods));
            }
        }

        public TypeSpecifier Type
        {
            get => cls?.Type;
        }

        public string FullName
        {
            get => cls?.FullName;
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
            get => cls?.Name;
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
                    OnPropertyChanged(nameof(SuperType));
                    OnPropertyChanged(nameof(OverridableMethods));
                    OnPropertyChanged(nameof(Type));
                    OnPropertyChanged(nameof(Modifiers));
                    OnPropertyChanged(nameof(Name));
                    OnPropertyChanged(nameof(StoragePath));
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        /// <summary>
        /// Path where the class is stored.
        /// </summary>
        public string StoragePath
        {
            get => $"{Class.FullName}.netpc";
        }

        /// <summary>
        /// Generated code for the current class.
        /// </summary>
        public string GeneratedCode
        {
            get
            {
                return generatedCode;
            }
            set
            {
                if (generatedCode != value)
                {
                    generatedCode = value;
                    OnPropertyChanged();
                }
            }
        }

        private string generatedCode;

        private Class cls;

        private ClassTranslator classTranslator = new ClassTranslator();

        public ClassVM(Class cls)
        {
            Class = cls;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Start();
            timer.Tick += (sender, eventArgs) =>
            {
                timer.Stop();

                string code;

                try
                {
                    code = classTranslator.TranslateClass(Class);
                }
                catch (Exception ex)
                {
                    code = ex.ToString();
                }

                GeneratedCode = code;

                timer.Start();
            };
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
