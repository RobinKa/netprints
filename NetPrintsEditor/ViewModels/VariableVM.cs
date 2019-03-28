using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NetPrintsEditor.ViewModels
{
    public class VariableVM : INotifyPropertyChanged
    {
        public TypeSpecifier VariableType
        {
            get => variable.VariableType;
            set
            {
                if (variable.VariableType != value)
                {
                    variable.VariableType = value;
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

        public string VisibilityName
        {
            get => Enum.GetName(typeof(MemberVisibility), Visibility);
            set => Visibility = Enum.Parse<MemberVisibility>(value);
        }

        public IEnumerable<string> PossibleVisibilities
        {
            get => new string[]
                {
                    Enum.GetName(typeof(MemberVisibility), MemberVisibility.Internal),
                    Enum.GetName(typeof(MemberVisibility), MemberVisibility.Private),
                    Enum.GetName(typeof(MemberVisibility), MemberVisibility.Protected),
                    Enum.GetName(typeof(MemberVisibility), MemberVisibility.Public),
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

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
