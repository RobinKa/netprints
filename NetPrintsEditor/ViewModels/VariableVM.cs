using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NetPrintsEditor.ViewModels
{
    public class VariableVM : INotifyPropertyChanged
    {
        public TypeSpecifier Type
        {
            get => variable.Type;
            set
            {
                if (variable.Type != value)
                {
                    variable.Type = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => variable.Name;
            set
            {
                if (variable.Name != value)
                {
                    variable.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public VariableModifiers Modifiers
        {
            get => variable.Modifiers;
            set
            {
                if (variable.Modifiers != value)
                {
                    variable.Modifiers = value;
                    OnPropertyChanged();
                }
            }
        }

        public MemberVisibility Visibility
        {
            get => variable.Visibility;
            set
            {
                if (variable.Visibility != value)
                {
                    variable.Visibility = value;
                    OnPropertyChanged();
                }
            }
        }

        public VariableSpecifier Specifier
        {
            get => variable.Specifier;
        }

        public bool HasGetter
        {
            get => variable.GetterMethod != null;
        }

        public bool HasSetter
        {
            get => variable.SetterMethod != null;
        }

        public Method GetterMethod
        {
            get => variable.GetterMethod;
        }

        public Method SetterMethod
        {
            get => variable.SetterMethod;
        }

        public string VisibilityName
        {
            get => Enum.GetName(typeof(MemberVisibility), Visibility);
            set => Visibility = Enum.Parse<MemberVisibility>(value);
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

        public Variable Variable
        {
            get => variable;
            set
            {
                if (variable != value)
                {
                    variable = value;
                    OnPropertyChanged();
                }
            }
        }

        private Variable variable;

        public VariableVM(Variable variable)
        {
            Variable = variable;
        }

        public void AddGetter()
        {
            var method = new Method($"get_{Name}")
            {
                Class = variable.Class,
            };

            // TODO

            variable.GetterMethod = method;
            OnPropertyChanged(nameof(HasGetter));
            OnPropertyChanged(nameof(GetterMethod));
        }

        public void RemoveGetter()
        {
            variable.GetterMethod = null;
            OnPropertyChanged(nameof(HasGetter));
            OnPropertyChanged(nameof(GetterMethod));
        }

        public void AddSetter()
        {
            var method = new Method($"set_{Name}")
            {
                Class = variable.Class,
            };

            // TODO

            variable.SetterMethod = method;
            OnPropertyChanged(nameof(HasSetter));
            OnPropertyChanged(nameof(SetterMethod));
        }

        public void RemoveSetter()
        {
            variable.SetterMethod = null;
            OnPropertyChanged(nameof(HasSetter));
            OnPropertyChanged(nameof(SetterMethod));
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
