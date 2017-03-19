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

        public ObservableViewModelCollection<NodeVM, Node> Nodes
        {
            get => nodes;
            set
            {
                if(nodes != value)
                {
                    nodes = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableViewModelCollection<NodeVM, Node> nodes;

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
                if (method.Modifiers != value)
                {
                    method.Modifiers = value;
                    OnPropertyChanged();
                }
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
            Method = method;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == nameof(Method))
            {
                Nodes = new ObservableViewModelCollection<NodeVM, Node>(Method.Nodes, n => new NodeVM(n));

                // Setup connections from new PinVMs to PinVMs
                foreach (NodeVM node in Nodes)
                {
                    foreach (NodePinVM pinVM in node.OutputExecPins)
                    {
                        NodeOutputExecPin pin = pinVM.Pin as NodeOutputExecPin;

                        if (pin.OutgoingPin != null)
                        {
                            NodeInputExecPin connPin = pin.OutgoingPin as NodeInputExecPin;
                            pinVM.ConnectedPin = Nodes.Where(n => n.Node == connPin.Node).Single().
                                InputExecPins.Single(x => x.Pin == connPin);
                        }
                    }

                    foreach (NodePinVM pinVM in node.InputDataPins)
                    {
                        NodeInputDataPin pin = pinVM.Pin as NodeInputDataPin;

                        if (pin.IncomingPin != null)
                        {
                            NodeOutputDataPin connPin = pin.IncomingPin as NodeOutputDataPin;
                            pinVM.ConnectedPin = Nodes.Where(n => n.Node == connPin.Node).Single().
                                OutputDataPins.Single(x => x.Pin == connPin);
                        }
                    }
                }
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
