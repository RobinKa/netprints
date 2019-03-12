using NetPrints.Core;
using NetPrints.Graph;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Linq;
using System;

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

        private static readonly SolidColorBrush MakeDelegateNodeBrush =
            new SolidColorBrush(Color.FromArgb(0xFF, 0x7A, 0x7A, 0x20));

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
                else if(Node is CallMethodNode callMethodNode)
                {
                    if (callMethodNode.IsStatic)
                    {
                        return CallStaticFunctionBrush;
                    }
                    else
                    {
                        return CallMethodBrush;
                    }
                }
                else if(Node is ConstructorNode)
                {
                    return ConstructorNodeBrush;
                }
                else if(Node is MakeDelegateNode)
                {
                    return MakeDelegateNodeBrush;
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

        public int ZIndex
        {
            get => IsSelected ? 1 : 0;
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
                    OnPropertyChanged(nameof(ZIndex));
                }
            }
        }

        private bool isSelected;

        public string ToolTip
        {
            get
            {
                if(Node is CallMethodNode callMethodNode)
                {
                    return ProjectVM.Instance.ReflectionProvider.GetMethodDocumentation(callMethodNode.MethodSpecifier);
                }

                return null;
            }
        }


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
                    OnPropertyChanged(nameof(ToolTip));
                    OnPropertyChanged(nameof(Overloads));
                }
            }
        }

        /// <summary>
        /// Overloads for Constructor and CallMethod nodes
        /// </summary>
        public ObservableRangeCollection<object> Overloads
        {
            get => overloads;
            set
            {
                overloads = value;
                OnPropertyChanged();
            }
        }
        private ObservableRangeCollection<object> overloads = new ObservableRangeCollection<object>();
        
        /// <summary>
        /// Currently overload or null if invalid for the node type.
        /// </summary>
        public object CurrentOverload
        {
            get
            {
                if (Node is CallMethodNode callMethodNode)
                {
                    return callMethodNode.MethodSpecifier;
                }
                else if (Node is ConstructorNode constructorNode)
                {
                    return constructorNode.ConstructorSpecifier;
                }

                return null;
            }
        }

        /// <summary>
        /// Changes the called method of this node if it is a CallMethodNode.
        /// Throws an exception if it is not.
        /// </summary>
        /// <param name="methodSpecifier">Method to change to.</param>
        public void ChangeOverload(object overload)
        {
            Node newNode = null;

            if (overload is MethodSpecifier methodSpecifier && Node is CallMethodNode callMethodNode)
            {
                newNode = new CallMethodNode(Node.Method, methodSpecifier);
            }
            else if (overload is ConstructorSpecifier constructorSpecifier && Node is ConstructorNode constructorNode)
            {
                newNode = new ConstructorNode(Node.Method, constructorSpecifier);
            }

            if (newNode != null)
            {
                // Remember old exec pins to reconnect them.
                // Data pin's are trickier to reconnect (or impossible).
                // They could be reconnected by heuristics (eg. name, type etc.).
                NodeOutputExecPin[] oldIncomingPins = Node.InputExecPins[0].IncomingPins.ToArray();
                NodeInputExecPin oldOutgoingPin = Node.OutputExecPins[0].OutgoingPin;

                // Disconnect the old node from other nodes and remove it
                GraphUtil.DisconnectNodePins(Node);
                Node.Method.Nodes.Remove(Node);

                // Move the new node to the same location
                newNode.PositionX = Node.PositionX;
                newNode.PositionY = Node.PositionY;

                // Reconnect execution pins
                if (oldOutgoingPin != null)
                {
                    GraphUtil.ConnectExecPins(newNode.OutputExecPins[0], oldOutgoingPin);
                }

                foreach (NodeOutputExecPin oldIncomingPin in oldIncomingPins)
                {
                    GraphUtil.ConnectExecPins(oldIncomingPin, newNode.InputExecPins[0]);
                }

                // Set the node of this view model which will trigger an update
                Node = newNode;
            }
            else
            {
                throw new Exception("Tried to change overload for underlying node even though it does not support overloads.");
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
            UpdateOverloads();
        }

        private void UpdateOverloads()
        {
            // Get the new overloads. Exclude the current method.
            if (node is CallMethodNode callMethodNode && callMethodNode.MethodSpecifier != null)
            {
                Overloads.ReplaceRange(ProjectVM.Instance.ReflectionProvider
                    .GetPublicMethodOverloads(callMethodNode.MethodSpecifier)
                    .Except(new MethodSpecifier[] { callMethodNode.MethodSpecifier }));
            }
            else if (node is ConstructorNode constructorNode && constructorNode.ConstructorSpecifier != null)
            {
                Overloads.ReplaceRange(ProjectVM.Instance.ReflectionProvider
                    .GetConstructors(constructorNode.ConstructorSpecifier.DeclaringType)
                    .Except(new ConstructorSpecifier[] { constructorNode.ConstructorSpecifier }));
            }
            else
            {
                Overloads.Clear();
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            UpdateOverloads();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
