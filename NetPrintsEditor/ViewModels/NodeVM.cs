using NetPrints.Core;
using NetPrints.Graph;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NetPrintsEditor.ViewModels
{
    public class NodeVM : INotifyPropertyChanged
    {
        // Wrapped attributes of Node
        public string Name
        {
            get => node.Name;
            set
            {
                if (node.Name != value)
                {
                    node.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableViewModelCollection<NodePinVM, NodeInputDataPin> InputDataPins
        {
            get => inputDataPins;
            set
            {
                if(inputDataPins != value)
                {
                    inputDataPins = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableViewModelCollection<NodePinVM, NodeOutputDataPin> OutputDataPins
        {
            get => outputDataPins;
            set
            {
                if (outputDataPins != value)
                {
                    outputDataPins = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableViewModelCollection<NodePinVM, NodeInputExecPin> InputExecPins
        {
            get => inputExecPins;
            set
            {
                if (inputExecPins != value)
                {
                    inputExecPins = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableViewModelCollection<NodePinVM, NodeOutputExecPin> OutputExecPins
        {
            get => outputExecPins;
            set
            {
                if (outputExecPins != value)
                {
                    outputExecPins = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableViewModelCollection<NodePinVM, NodeInputDataPin> inputDataPins;
        private ObservableViewModelCollection<NodePinVM, NodeOutputDataPin> outputDataPins;
        private ObservableViewModelCollection<NodePinVM, NodeInputExecPin> inputExecPins;
        private ObservableViewModelCollection<NodePinVM, NodeOutputExecPin> outputExecPins;
        
        public bool IsPure { get => node.IsPure; }

        // Wrapped Node
        public Node Node
        {
            get => node;
            set
            {
                if (node != value)
                {
                    node = value;
                    OnPropertyChanged();
                }
            }
        }

        public Method Method
        {
            get => node.Method;
        }

        public double PositionX
        {
            get => node.PositionX;
            set
            {
                if (node.PositionX != value)
                {
                    node.PositionX = value;
                    OnPropertyChanged();
                }
            }
        }

        public double PositionY
        {
            get => node.PositionY;
            set
            {
                if (node.PositionY != value)
                {
                    node.PositionY = value;
                    OnPropertyChanged();
                }
            }
        }

        private Node node;

        public NodeVM(Node node)
        {
            Node = node;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if(propertyName == nameof(Node))
            {
                InputDataPins = new ObservableViewModelCollection<NodePinVM, NodeInputDataPin>(
                    Node.InputDataPins, p => new NodePinVM(p));

                OutputDataPins = new ObservableViewModelCollection<NodePinVM, NodeOutputDataPin>(
                    Node.OutputDataPins, p => new NodePinVM(p));

                InputExecPins = new ObservableViewModelCollection<NodePinVM, NodeInputExecPin>(
                    Node.InputExecPins, p => new NodePinVM(p));

                OutputExecPins = new ObservableViewModelCollection<NodePinVM, NodeOutputExecPin>(
                    Node.OutputExecPins, p => new NodePinVM(p));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
