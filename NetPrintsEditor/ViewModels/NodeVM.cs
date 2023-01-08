using NetPrints.Core;
using NetPrints.Graph;
using System.ComponentModel;
using System.Windows.Media;
using System.Linq;
using System;
using GalaSoft.MvvmLight;
using NetPrintsEditor.Messages;
using NetPrints.Base;

namespace NetPrintsEditor.ViewModels
{
    public class NodeVM : ViewModelBase
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

        private static readonly SolidColorBrush TypeNodeBrush =
            new SolidColorBrush(Color.FromArgb(0xFF, 0x7A, 0x30, 0x20));

        private static readonly SolidColorBrush VariableGetterBrush =
            new SolidColorBrush(Color.FromArgb(0xFF, 0x30, 0x5A, 0x5A));

        private static readonly SolidColorBrush VariableSetterBrush =
            new SolidColorBrush(Color.FromArgb(0xFF, 0x5A, 0x5A, 0x7A));

        private static readonly SolidColorBrush MakeArrayBrush =
            new SolidColorBrush(Color.FromArgb(0xFF, 0x1A, 0x5A, 0x30));

        private static readonly SolidColorBrush ThrowBrush =
            new SolidColorBrush(Color.FromArgb(0xFF, 0xBB, 0x20, 0x20));

        private static readonly SolidColorBrush TernaryBrush =
            new SolidColorBrush(Color.FromArgb(0xFF, 0x40, 0x3A, 0x3A));

        /// <summary>
        /// Brush for the header of the node.
        /// </summary>
        public SolidColorBrush Brush
        {
            get
            {
                if (Node is ExecutionEntryNode)
                {
                    return EntryNodeBrush;
                }
                else if (Node is ReturnNode)
                {
                    return ReturnNodeBrush;
                }
                else if (Node is CallMethodNode callMethodNode)
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
                else if (Node is ConstructorNode)
                {
                    return ConstructorNodeBrush;
                }
                else if (Node is MakeDelegateNode)
                {
                    return MakeDelegateNodeBrush;
                }
                else if (Node is TypeNode || Node is MakeArrayTypeNode)
                {
                    return TypeNodeBrush;
                }
                else if (Node is VariableGetterNode)
                {
                    return VariableGetterBrush;
                }
                else if (Node is VariableSetterNode)
                {
                    return VariableSetterBrush;
                }
                else if (Node is MakeArrayNode)
                {
                    return MakeArrayBrush;
                }
                else if (Node is ThrowNode)
                {
                    return ThrowBrush;
                }
                else if (Node is TernaryNode)
                {
                    return TernaryBrush;
                }

                return DefaultNodeBrush;
            }
        }

        private static readonly SolidColorBrush DeselectedBorderBrush =
            new SolidColorBrush(Color.FromArgb(0xCC, 0x30, 0x30, 0x30));

        private static readonly SolidColorBrush SelectedBorderBrush =
            new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x99, 0x00));

        /// <summary>
        /// Brush for the border of the node.
        /// </summary>
        public SolidColorBrush BorderBrush
        {
            get => IsSelected ? SelectedBorderBrush : DeselectedBorderBrush;
        }

        public int ZIndex
        {
            get => IsSelected ? 1 : 0;
        }

        public string LeftPlusToolTip
        {
            get
            {
                if (node is MakeArrayNode)
                {
                    return "Add array element";
                }
                else if (node is MethodEntryNode)
                {
                    return "Add method parameter";
                }
                else if (node is ReturnNode)
                {
                    return "Add method return value";
                }
                else if (node is ClassReturnNode)
                {
                    return "Add interface";
                }

                return "";
            }
        }

        public string LeftMinusToolTip
        {
            get
            {
                if (node is MakeArrayNode)
                {
                    return "Remove array element";
                }
                else if (node is MethodEntryNode)
                {
                    return "Remove method parameter";
                }
                else if (node is ReturnNode)
                {
                    return "Remove method return value";
                }
                else if (node is ClassReturnNode)
                {
                    return "Remove interface";
                }

                return "";
            }
        }

        public string RightPlusToolTip
        {
            get
            {
                if (node is MethodEntryNode)
                {
                    return "Add method generic type parameter";
                }

                return "";
            }
        }

        public string RightMinusToolTip
        {
            get
            {
                if (node is MethodEntryNode)
                {
                    return "Remove method generic type parameter";
                }

                return "";
            }
        }

        /// <summary>
        /// Whether the node is currently selected.
        /// </summary>
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(BorderBrush));
                    RaisePropertyChanged(nameof(ZIndex));
                }
            }
        }

        private bool isSelected;

        /// <summary>
        /// Whether this node is a reroute node.
        /// </summary>
        public bool IsRerouteNode
        {
            get => node is RerouteNode;
        }

        /// <summary>
        /// Tool tip of the node shown when hovering over it.
        /// </summary>
        public string ToolTip
        {
            get
            {
                if (Node is CallMethodNode callMethodNode)
                {
                    return App.ReflectionProvider.GetMethodDocumentation(callMethodNode.MethodSpecifier);
                }

                return null;
            }
        }

        // Wrapped attributes of Node

        /// <summary>
        /// Name of the node.
        /// </summary>
        public string Name
        {
            get => node.Name;
            set
            {
                if (node.Name != value)
                {
                    node.Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableViewModelCollection<NodePinVM, INodePin> Pins { get; private set; }
        public IObservableCollectionView<NodePinVM> InputExecPins { get; private set; }
        public IObservableCollectionView<NodePinVM> OutputExecPins { get; private set; }
        public IObservableCollectionView<NodePinVM> InputDataPins { get; private set; }
        public IObservableCollectionView<NodePinVM> OutputDataPins { get; private set; }
        public IObservableCollectionView<NodePinVM> InputTypePins { get; private set; }
        public IObservableCollectionView<NodePinVM> OutputTypePins { get; private set; }

        public bool IsPure
        {
            get => node.IsPure;
            set => node.IsPure = value;
        }

        public bool CanSetPure
        {
            get => node.CanSetPure;
        }

        // Wrapped Node
        public Node Node
        {
            get => node;
            set
            {
                if (node != value)
                {
                    if (node != null)
                    {
                        node.InputTypeChanged -= OnInputTypeChanged;
                    }

                    node = value;

                    if (node != null)
                    {
                        node.InputTypeChanged += OnInputTypeChanged;

                        Pins = new ObservableViewModelCollection<NodePinVM, INodePin>(Node.Pins, p => new NodePinVM((NodePin)p));
                        InputExecPins = Pins.ObservableWhere(pinViewModel => pinViewModel.Pin is NodeInputExecPin);
                        OutputExecPins = Pins.ObservableWhere(pinViewModel => pinViewModel.Pin is NodeOutputExecPin);
                        InputDataPins = Pins.ObservableWhere(pinViewModel => pinViewModel.Pin is NodeInputDataPin);
                        OutputDataPins = Pins.ObservableWhere(pinViewModel => pinViewModel.Pin is NodeOutputDataPin);
                        InputTypePins = Pins.ObservableWhere(pinViewModel => pinViewModel.Pin is NodeInputTypePin);
                        OutputTypePins = Pins.ObservableWhere(pinViewModel => pinViewModel.Pin is NodeOutputTypePin);
                    }

                    UpdateOverloads();

                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Brush));
                    RaisePropertyChanged(nameof(ToolTip));
                    RaisePropertyChanged(nameof(IsRerouteNode));
                    RaisePropertyChanged(nameof(ShowLeftPinButtons));
                    RaisePropertyChanged(nameof(ShowRightPinButtons));
                    RaisePropertyChanged(nameof(LeftPlusToolTip));
                    RaisePropertyChanged(nameof(LeftMinusToolTip));
                    RaisePropertyChanged(nameof(RightPlusToolTip));
                    RaisePropertyChanged(nameof(RightMinusToolTip));
                    RaisePropertyChanged(nameof(Label));
                    RaisePropertyChanged(nameof(IsPure));
                    RaisePropertyChanged(nameof(CanSetPure));
                }
            }
        }

        private void OnInputTypeChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(Label));
        }

        public string Label
        {
            get => Node.ToString();
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
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ShowOverloads));
            }
        }

        private ObservableRangeCollection<object> overloads = new ObservableRangeCollection<object>();

        /// <summary>
        /// Whether to show the overloads element.
        /// </summary>
        public bool ShowOverloads
        {
            get => Overloads.Count > 0;
        }

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
                else if (Node is MakeArrayNode makeArrayNode)
                {
                    return makeArrayNode.UsePredefinedSize ? "Use initializer list" : "Use predefined size";
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
            if (overload is MethodSpecifier methodSpecifier && Node is CallMethodNode)
            {
                newNode = new CallMethodNode(Node.Graph, methodSpecifier);
            }
            else if (overload is ConstructorSpecifier constructorSpecifier && Node is ConstructorNode)
            {
                newNode = new ConstructorNode(Node.Graph, constructorSpecifier);
            }
            else if (overload is string overloadString && Node is MakeArrayNode makeArrayNode)
            {
                makeArrayNode.UsePredefinedSize = string.Equals(overloadString, "Use predefined size", StringComparison.OrdinalIgnoreCase);
                UpdateOverloads();
            }
            else
            {
                throw new Exception("Tried to change overload for underlying node even though it does not support overloads.");
            }

            if (newNode != null)
            {
                // Remember old exec pins to reconnect them.
                // Data pin's are trickier to reconnect (or impossible).
                // They could be reconnected by heuristics (eg. name, type etc.).

                NodeOutputExecPin[] oldIncomingPins = null;
                NodeInputExecPin oldOutgoingPin = null;

                bool oldPurity = Node.IsPure;
                if (!oldPurity)
                {
                    oldIncomingPins = Node.InputExecPins[0].IncomingExecutionPins.ToArray();
                    oldOutgoingPin = Node.OutputExecPins[0].OutgoingExecPin;
                }

                // Disconnect the old node from other nodes and remove it
                GraphUtil.DisconnectNodePins(Node);
                Node.Graph.Nodes.Remove(Node);

                // Move the new node to the same location
                newNode.PositionX = Node.PositionX;
                newNode.PositionY = Node.PositionY;

                // Restore old purity
                if (newNode.CanSetPure)
                {
                    newNode.IsPure = oldPurity;
                }

                // Reconnect execution pins
                if (!newNode.IsPure)
                {
                    if (oldOutgoingPin != null)
                    {
                        GraphUtil.ConnectPins(newNode.OutputExecPins[0], oldOutgoingPin);
                    }

                    if (oldIncomingPins != null)
                    {
                        foreach (NodeOutputExecPin oldIncomingPin in oldIncomingPins)
                        {
                            GraphUtil.ConnectPins(oldIncomingPin, newNode.InputExecPins[0]);
                        }
                    }
                }

                // Set the node of this view model which will trigger an update
                Node = newNode;
            }
        }

        public MethodGraph Method
        {
            get => node.MethodGraph;
        }

        public NodeGraph Graph
        {
            get => node.Graph;
        }

        /// <summary>
        /// Whether we should show the +/- buttons under the
        /// left pins.
        /// </summary>
        public bool ShowLeftPinButtons
        {
            get => node is INodeInputButtons;
        }

        /// <summary>
        /// Whether we should show the +/- buttons under the
        /// right pins.
        /// </summary>
        public bool ShowRightPinButtons
        {
            get => node is INodeOutputButtons;
        }

        /// <summary>
        /// Called when the left pins' plus button was clicked.
        /// </summary>
        public void LeftPinsPlusClicked()
        {
            (node as INodeInputButtons)?.InputPlusClicked();
        }

        /// <summary>
        /// Called when the left pins' minus button was clicked.
        /// </summary>
        public void LeftPinsMinusClicked()
        {
            (node as INodeInputButtons)?.InputMinusClicked();
        }

        /// <summary>
        /// Called when the right pins' plus button was clicked.
        /// </summary>
        public void RightPinsPlusClicked()
        {
            (node as INodeOutputButtons)?.OutputPlusClicked();
        }

        /// <summary>
        /// Called when the right pins' minus button was clicked.
        /// </summary>
        public void RightPinsMinusClicked()
        {
            (node as INodeOutputButtons)?.OutputMinusClicked();
        }

        private Node node;

        public NodeVM(Node node)
        {
            PropertyChanged += OnPropertyChanged;
            Node = node;
        }

        private void UpdateOverloads()
        {
            // Get the new overloads. Exclude the current method.
            if (node is CallMethodNode callMethodNode && callMethodNode.MethodSpecifier != null)
            {
                Overloads.ReplaceRange(App.ReflectionProvider
                    .GetPublicMethodOverloads(callMethodNode.MethodSpecifier)
                    .Except(new MethodSpecifier[] { callMethodNode.MethodSpecifier }));
            }
            else if (node is ConstructorNode constructorNode && constructorNode.ConstructorSpecifier != null)
            {
                Overloads.ReplaceRange(App.ReflectionProvider
                    .GetConstructors(constructorNode.ConstructorSpecifier.DeclaringType)
                    .Except(new ConstructorSpecifier[] { constructorNode.ConstructorSpecifier }));
            }
            else if (node is MakeArrayNode makeArrayNode)
            {
                if (makeArrayNode.UsePredefinedSize)
                {
                    Overloads.Replace("Use initializer list");
                }
                else
                {
                    Overloads.Replace("Use predefined size");
                }
            }
            else
            {
                Overloads.Clear();
            }

            RaisePropertyChanged(nameof(ShowOverloads));
            RaisePropertyChanged(nameof(Overloads));
        }

        #region Dragging
        public delegate void NodeDragStartEventHandler(NodeVM node);
        public delegate void NodeDragEndEventHandler(NodeVM node);
        public delegate void NodeDragMoveEventHandler(NodeVM node, double dx, double dy);
        public event NodeDragStartEventHandler OnDragStart;
        public event NodeDragEndEventHandler OnDragEnd;
        public event NodeDragMoveEventHandler OnDragMove;

        public void DragStart() => OnDragStart?.Invoke(this);
        public void DragEnd() => OnDragEnd?.Invoke(this);
        public void DragMove(double dx, double dy) => OnDragMove?.Invoke(this, dx, dy);
        #endregion

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // HACK: call UpdateOverloads() here because for some reason it is not
            //       updated correctly.
            //if (!e.PropertyName.Contains("overload", StringComparison.OrdinalIgnoreCase))
            if (!e.PropertyName.Contains("Overload"))
            {
                UpdateOverloads();
            }
        }

        public void Select()
        {
            MessengerInstance.Send(new NodeSelectionMessage(this));
        }
    }
}
