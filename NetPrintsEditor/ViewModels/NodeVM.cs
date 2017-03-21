using NetPrints.Core;
using NetPrints.Graph;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Linq;

namespace NetPrintsEditor.ViewModels
{
    public class NodeVM : INotifyPropertyChanged
    {
        private static readonly SolidColorBrush DefaultNodeBrush =
            new SolidColorBrush(Color.FromArgb(0xFF, 0x30, 0x30, 0x30));

        private static readonly SolidColorBrush EntryNodeBrush =
            new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0x20, 0x50));

        private static readonly SolidColorBrush ReturnNodeBrush =
            new SolidColorBrush(Color.FromArgb(0xFF, 0x50, 0x20, 0x20));

        private static readonly SolidColorBrush CallMethodBrush =
            new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0x3A, 0x50));

        private static readonly SolidColorBrush CallStaticFunctionBrush =
            new SolidColorBrush(Color.FromArgb(0xFF, 0x50, 0x20, 0x3A));

        private static readonly SolidColorBrush ConstructorNodeBrush =
            new SolidColorBrush(Color.FromArgb(0xFF, 0x3A, 0x50, 0x20));

        public SolidColorBrush Brush
        {
            get
            {
                if(Node is EntryNode)
                {
                    return EntryNodeBrush;
                }
                else if(Node is ReturnNode)
                {
                    return ReturnNodeBrush;
                }
                else if(Node is CallMethodNode)
                {
                    return CallMethodBrush;
                }
                else if(Node is CallStaticFunctionNode)
                {
                    return CallStaticFunctionBrush;
                }
                else if(Node is ConstructorNode)
                {
                    return ConstructorNodeBrush;
                }

                return DefaultNodeBrush;
            }
        }

        private static readonly SolidColorBrush DeselectedBorderBrush =
            new SolidColorBrush(Color.FromArgb(0xCC, 0x30, 0x30, 0x30));

        private static readonly SolidColorBrush SelectedBorderBrush =
            new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xEE, 0x50));

        public SolidColorBrush BorderBrush
        {
            get => IsSelected ? SelectedBorderBrush : DeselectedBorderBrush;
        }

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BorderBrush));
                }
            }
        }

        private bool isSelected;

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

                    InputDataPins = new ObservableViewModelCollection<NodePinVM, NodeInputDataPin>(
                        Node.InputDataPins, p => new NodePinVM(p));

                    OutputDataPins = new ObservableViewModelCollection<NodePinVM, NodeOutputDataPin>(
                        Node.OutputDataPins, p => new NodePinVM(p));

                    InputExecPins = new ObservableViewModelCollection<NodePinVM, NodeInputExecPin>(
                        Node.InputExecPins, p => new NodePinVM(p));

                    OutputExecPins = new ObservableViewModelCollection<NodePinVM, NodeOutputExecPin>(
                        Node.OutputExecPins, p => new NodePinVM(p));
                    
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Brush));
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
