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
    public class ClassEditorVM : INotifyPropertyChanged
    {
        public Project Project
        {
            get => Class.Project;
        }

        // Wrapped attributes of Class
        public ObservableViewModelCollection<MemberVariableVM, Variable> Variables
        {
            get => variables;
            set
            {
                if (variables != value)
                {
                    variables = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableViewModelCollection<MemberVariableVM, Variable> variables;

        public ObservableViewModelCollection<NodeGraphVM, MethodGraph> Methods
        {
            get => methods;
            set
            {
                if (methods != value)
                {
                    methods = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableViewModelCollection<NodeGraphVM, MethodGraph> methods;

        public ObservableViewModelCollection<NodeGraphVM, ConstructorGraph> Constructors
        {
            get => constructors;
            set
            {
                if (constructors != value)
                {
                    constructors = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableViewModelCollection<NodeGraphVM, ConstructorGraph> constructors;

        /// <summary>
        /// Specifiers for methods that this class can override.
        /// </summary>
        public IEnumerable<MethodSpecifier> OverridableMethods
        {
            get => App.ReflectionProvider.GetOverridableMethodsForType(Class.SuperType);
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
                OnPropertyChanged(nameof(FullName));
                OnPropertyChanged(nameof(Type));

                foreach (NodeGraphVM constructor in Constructors)
                {
                    constructor.Name = cls.Name;
                }
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

        public ClassGraph Class
        {
            get => cls;
            set
            {
                if (cls != value)
                {
                    cls = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(OverridableMethods));
                    OnPropertyChanged(nameof(Type));
                    OnPropertyChanged(nameof(Modifiers));
                    OnPropertyChanged(nameof(Name));
                    OnPropertyChanged(nameof(FullName));
                    OnPropertyChanged(nameof(Visibility));
                }
            }
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

        private ClassGraph cls;

        private readonly ClassTranslator classTranslator = new ClassTranslator();

        private readonly Timer codeTimer;

        public ClassEditorVM(ClassGraph cls)
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

        ~ClassEditorVM()
        {
            codeTimer?.Stop();
        }

#region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == nameof(Class))
            {
                Methods = new ObservableViewModelCollection<NodeGraphVM, MethodGraph>(
                    cls.Methods, m => new NodeGraphVM(m) { Class = this } );

                Constructors = new ObservableViewModelCollection<NodeGraphVM, ConstructorGraph>(
                    cls.Constructors, c => new NodeGraphVM(c) { Class = this });

                Variables = new ObservableViewModelCollection<MemberVariableVM, Variable>(
                    cls.Variables, v => new MemberVariableVM(v));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
#endregion
    }
}
