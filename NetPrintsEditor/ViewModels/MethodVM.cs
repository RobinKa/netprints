using NetPrints.Core;
using NetPrints.Graph;
using NetPrints.Translator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NetPrintsEditor.ViewModels
{
    public class MethodVM : INotifyPropertyChanged
    {
        public string Name
        {
            get => method.Name;
            set
            {
                if(method.Name != value)
                {
                    method.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Node> Nodes { get => method.Nodes; }

        public ObservableCollection<Type> ArgumentTypes
        {
            get => method.ArgumentTypes;
        }

        public ObservableCollection<Type> ReturnTypes
        {
            get => method.ReturnTypes;
        }

        public MethodModifiers Modifiers
        {
            get => method.Modifiers;
            set
            {
                method.Modifiers = value;
                OnPropertyChanged();
            }
        }

        public Method Method
        {
            get => method;
            set
            {
                if (method != value)
                {
                    method = value;
                    OnPropertyChanged();
                }
            }
        }

        private Method method;

        public MethodVM(Method method)
        {
            this.method = method;
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
