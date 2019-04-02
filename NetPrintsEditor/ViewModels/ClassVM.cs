using NetPrints.Core;
using NetPrints.Translator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
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
        public ObservableViewModelCollection<VariableVM, Variable> Variables
        {
            get => variables;
            set
            {
                if(variables != value)
                {
                    variables = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableViewModelCollection<VariableVM, Variable> variables;

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
                if (!(value is null))
                {
                    cls.SuperType = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(OverridableMethods));
                }
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

        public MemberVisibility Visibility
        {
            get => cls.Visibility;
            set
            {
                if (cls.Visibility != value)
                {
                    cls.Visibility = value;
                    OnPropertyChanged();
                }
            }
        }

        public IEnumerable<MemberVisibility> PossibleVisibilities
        {
            get => new[]
                {
                    MemberVisibility.Internal,
                    MemberVisibility.Private,
                    MemberVisibility.Protected,
                    MemberVisibility.Public,
                };
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
                    OnPropertyChanged(nameof(Visibility));
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

        private Timer codeTimer;

        public ClassVM(Class cls)
        {
            Class = cls;

            codeTimer = new Timer(1000);
            codeTimer.Elapsed += (sender, eventArgs) =>
            {
                codeTimer.Stop();

                string code;

                try
                {
                    code = classTranslator.TranslateClass(Class);
                }
                catch (Exception ex)
                {
                    code = ex.ToString();
                }

                Dispatcher.CurrentDispatcher.Invoke(() => GeneratedCode = code);

                codeTimer.Start();
            };
            codeTimer.Start();
        }

        ~ClassVM()
        {
            codeTimer.Stop();
        }

#region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if(propertyName == nameof(Class))
            {
                Methods = new ObservableViewModelCollection<MethodVM, Method>(
                    cls.Methods, m => new MethodVM(m) { Class = this } );

                Variables = new ObservableViewModelCollection<VariableVM, Variable>(
                    cls.Variables, v => new VariableVM(v));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
#endregion
    }
}
