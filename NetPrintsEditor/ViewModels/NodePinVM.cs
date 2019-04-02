using NetPrints.Core;
using NetPrints.Graph;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace NetPrintsEditor.ViewModels
{
    public class NodePinVM : INotifyPropertyChanged
    {
        public string ToolTip
        {
            get
            {
                string toolTip = "";

                if (Pin is NodeDataPin dataPin)
                {
                    toolTip = $"{dataPin.PinType.Value}: {dataPin.Name}";
                    string documentation = null;

                    if (dataPin.Node is CallMethodNode callMethodNode)
                    {
                        if (dataPin is NodeInputDataPin inputDataPin)
                        {
                            int paramIndex = callMethodNode.ArgumentPins.IndexOf(inputDataPin);
                            if (paramIndex >= 0)
                            {
                                documentation = ProjectVM.Instance.ReflectionProvider.GetMethodParameterDocumentation(callMethodNode.MethodSpecifier, paramIndex);
                            }
                        }
                        else if(dataPin is NodeOutputDataPin outputDataPin)
                        {
                            int returnIndex = callMethodNode.OutputDataPins.IndexOf(outputDataPin);
                            if (returnIndex >= 0)
                            {
                                documentation = ProjectVM.Instance.ReflectionProvider.GetMethodReturnDocumentation(callMethodNode.MethodSpecifier, returnIndex);
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(documentation))
                    {
                        toolTip += Environment.NewLine + Environment.NewLine + documentation;
                    }
                }
                else if (Pin is NodeInputExecPin)
                {
                    toolTip = "Can be connected to output execution pins to receive execution.";
                }
                else if (Pin is NodeOutputExecPin)
                {
                    toolTip = "Can be connected to input execution pins to pass on execution.";

                    // TODO: Don't hardcode this / let execution pins have proper tooltips
                    if (Pin.Name == "Catch")
                    {
                        toolTip += Environment.NewLine + Environment.NewLine + "Executed when an exception is thrown on this node. The Exception output data pin will be set to the caught exception.";
                    }
                }
                else
                {
                    toolTip += Pin.Name;
                }

                return toolTip;
            }
        }

        public Point ConnectingAbsolutePosition
        {
            get => connectingRelativeMousePosition;
            set
            {
                if(connectingRelativeMousePosition != value)
                {
                    connectingRelativeMousePosition = value;
                    OnPropertyChanged();
                    OnConnectionPositionUpdate();
                }
            }
        }

        private Point connectingRelativeMousePosition;

        public bool IsBeingConnected
        {
            get => isBeingConnected;
            set
            {
                if(isBeingConnected != value)
                {
                    isBeingConnected = value;

                    // Disconnect pin if its being connected
                    // and is an IDP, OXP or ITP
                    if(value && (Pin is NodeInputDataPin || Pin is NodeOutputExecPin || Pin is NodeInputTypePin))
                    {
                        ConnectedPin = null;
                    }

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCableVisible));
                    OnConnectionPositionUpdate();
                }
            }
        }

        private bool isBeingConnected = false;

        public NodePin Pin
        {
            get => pin;
            set
            {
                if(pin != value)
                {
                    if(pin != null)
                    {
                        pin.Node.OnPositionChanged -= OnNodePositionChanged;
                        pin.Node.InputTypeChanged -= OnInputTypeChanged;
                    }

                    pin = value;

                    if (pin != null)
                    {
                        pin.Node.OnPositionChanged += OnNodePositionChanged;
                        pin.Node.InputTypeChanged += OnInputTypeChanged;
                    }

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FillBrush));
                    OnPropertyChanged(nameof(BorderBrush));
                    OnPropertyChanged(nameof(ShowUnconnectedValue));
                    OnPropertyChanged(nameof(ShowEnumValue));
                    OnPropertyChanged(nameof(ShowBooleanValue));
                    OnPropertyChanged(nameof(PossibleEnumNames));
                    OnPropertyChanged(nameof(ToolTip));
                    OnPropertyChanged(nameof(IsRerouteNodePin));
                }
            }
        }

        private void OnInputTypeChanged(object sender, EventArgs eventArgs)
        {
            OnPropertyChanged(nameof(PossibleEnumNames));
            OnPropertyChanged(nameof(ShowUnconnectedValue));
            OnPropertyChanged(nameof(ShowBooleanValue));
            OnPropertyChanged(nameof(ShowEnumValue));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(ToolTip));
        }

        public bool IsRerouteNodePin
        {
            get => Node is RerouteNode;
        }

        /// <summary>
        /// Disconnects the pin from all of its connections.
        /// </summary>
        public void DisconnectAll()
        {
            GraphUtil.DisconnectPin(pin);
        }

        /// <summary>
        /// Connects this pin to the first possible pin of the passed node.
        /// </summary>
        /// <param name="node">Node to connect this pin to.</param>
        public void ConnectRelevant(Node node)
        {
            if (pin is NodeInputExecPin ixp)
            {
                GraphUtil.ConnectExecPins(node.OutputExecPins[0], ixp);
            }
            else if (pin is NodeOutputExecPin oxp)
            {
                GraphUtil.ConnectExecPins(oxp, node.InputExecPins[0]);
            }
            else if (pin is NodeInputDataPin idp)
            {
                foreach (var otherOtp in node.OutputDataPins)
                {
                    if (GraphUtil.CanConnectNodePins(otherOtp, idp, ProjectVM.Instance.ReflectionProvider.TypeSpecifierIsSubclassOf, ProjectVM.Instance.ReflectionProvider.HasImplicitCast))
                    {
                        GraphUtil.ConnectDataPins(otherOtp, idp);

                        // Connect exec pins if possible.
                        // Also forward the previous connection through the new node.
                        if (pin.Node.InputExecPins.Count > 0 && node.OutputExecPins.Count > 0)
                        {
                            var oldConnected = pin.Node.InputExecPins[0].IncomingPins.FirstOrDefault();

                            if (oldConnected != null)
                            {
                                GraphUtil.DisconnectOutputExecPin(oldConnected);
                            }

                            GraphUtil.ConnectExecPins(node.OutputExecPins[0], pin.Node.InputExecPins[0]);

                            if (oldConnected != null && node.InputExecPins.Count > 0)
                            {
                                GraphUtil.ConnectExecPins(oldConnected, node.InputExecPins[0]);
                            }
                        }

                        break;
                    }
                }
            }
            else if (pin is NodeOutputDataPin odp)
            {
                foreach (var otherIdp in node.InputDataPins)
                {
                    if (GraphUtil.CanConnectNodePins(odp, otherIdp, ProjectVM.Instance.ReflectionProvider.TypeSpecifierIsSubclassOf, ProjectVM.Instance.ReflectionProvider.HasImplicitCast))
                    {
                        GraphUtil.ConnectDataPins(odp, otherIdp);

                        // Connect exec pins if possible.
                        // Also forward the previous connection through the new node.
                        if (node.InputExecPins.Count > 0 && pin.Node.OutputExecPins.Count > 0)
                        {
                            var oldConnected = pin.Node.OutputExecPins[0].OutgoingPin;

                            GraphUtil.ConnectExecPins(pin.Node.OutputExecPins[0], node.InputExecPins[0]);

                            if (oldConnected != null && node.OutputExecPins.Count > 0)
                            {
                                GraphUtil.ConnectExecPins(node.OutputExecPins[0], oldConnected);
                            }
                        }

                        break;
                    }
                }
            }
            else if (pin is NodeInputTypePin itp)
            {
                if (node.OutputTypePins.Count > 0)
                {
                    GraphUtil.ConnectTypePins(node.OutputTypePins[0], itp);
                }
            }
            else if (pin is NodeOutputTypePin otp)
            {
                if (node.InputTypePins.Count > 0)
                {
                    GraphUtil.ConnectTypePins(otp, node.InputTypePins[0]);
                }
            }
            else
            {
                throw new NotImplementedException("Unknown pin type");
            }
        }

        public object UnconnectedValue
        {
            get
            {
                if(Pin is NodeInputDataPin p)
                {
                    return p.UnconnectedValue;
                }

                return null;
            }
            set
            {
                if (Pin is NodeInputDataPin p && p.UnconnectedValue != value
                    && p.PinType.Value is TypeSpecifier typeSpecifier)
                {
                    // Try to convert to the correct type first if it can be found
                    // Dont do this for enums as they just use a string

                    Type t = Type.GetType(typeSpecifier.Name);
                    
                    if (t != null && !typeSpecifier.IsEnum)
                    {
                        try
                        {
                            p.UnconnectedValue = Convert.ChangeType(value, t);
                        }
                        catch (InvalidCastException)
                        {
                        }
                    }
                    else
                    {
                        p.UnconnectedValue = value;
                    }

                    OnPropertyChanged();
                }
            }
        }

        public bool ShowUnconnectedValue
        {
            get => Pin is NodeInputDataPin p && p.UsesUnconnectedValue && !IsConnected && 
                !(p.PinType.Value is TypeSpecifier typeSpec && (typeSpec.IsEnum || typeSpec == TypeSpecifier.FromType<bool>()));
        }

        public bool ShowEnumValue
        {
            get => Pin is NodeInputDataPin p && p.UsesUnconnectedValue && !IsConnected && 
                (p.PinType.Value is TypeSpecifier typeSpec && typeSpec.IsEnum);
        }

        public bool ShowBooleanValue
        {
            get => Pin is NodeInputDataPin p && p.UsesUnconnectedValue && !IsConnected &&
                p.PinType.Value is TypeSpecifier typeSpec &&
                typeSpec == TypeSpecifier.FromType<bool>();
        }

        public IEnumerable<string> PossibleEnumNames
        {
            get
            {
                if (Pin is NodeInputDataPin p && p.PinType.Value is TypeSpecifier typeSpec && typeSpec.IsEnum)
                {
                    return ProjectVM.Instance.ReflectionProvider.GetEnumNames(typeSpec);
                }

                return null;
            }
        }

        public Node Node
        {
            get => pin.Node;
        }

        public string DisplayName
        {
            get => pin.ToString();
        }

        /// <summary>
        /// Whether the name of this pin is editable.
        /// </summary>
        public bool IsNameEditable
        {
            get => (pin.Node is EntryNode && pin.Node.OutputDataPins.Contains(pin)) ||
                (pin.Node is ReturnNode && pin.Node.InputDataPins.Contains(pin));
        }

        public string Name
        {
            get => pin.Name;
            set
            {
                if (pin.Name != value)
                {
                    pin.Name = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        private void OnNodePositionChanged(Node node, double positionX, double positionY)
        {
            OnConnectionPositionUpdate();
        }
        
        public double PositionX
        {
            get => positionX;
            set
            {
                if(positionX != value)
                {
                    positionX = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Position));
                    OnPropertyChanged(nameof(AbsolutePosition));
                    OnConnectionPositionUpdate();
                }
            }
        }

        public double PositionY
        {
            get => positionY;
            set
            {
                if (positionY != value)
                {
                    positionY = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Position));
                    OnPropertyChanged(nameof(AbsolutePosition));
                    OnConnectionPositionUpdate();
                }
            }
        }

        public Point Position
        {
            get => new Point(PositionX, PositionY);
        }

        private static readonly Dictionary<Type, Brush> typeBrushes = new Dictionary<Type, Brush>()
        {
            [typeof(NodeExecPin)] = new SolidColorBrush(Color.FromArgb(0xFF, 0xE0, 0xFF, 0xE0)),
            [typeof(NodeDataPin)] = new SolidColorBrush(Color.FromArgb(0xFF, 0xE0, 0xE0, 0xFF)),
            [typeof(NodeTypePin)] = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xE0, 0xE0)),
        };

        public Brush BorderBrush
        {
            get => typeBrushes.Single(x => Pin.GetType().IsSubclassOf(x.Key)).Value;
        }

        public Brush FillBrush
        {
            get
            {
                // Check if the pin is connected to anything
                if ((pin is NodeInputDataPin idp && idp.IncomingPin != null) ||
                    (pin is NodeOutputDataPin odp && odp.OutgoingPins.Count > 0) ||
                    (pin is NodeInputExecPin iep && iep.IncomingPins.Count > 0) ||
                    (pin is NodeOutputExecPin oep && oep.OutgoingPin != null) ||
                    (pin is NodeInputTypePin itp && itp.IncomingPin != null) ||
                    (pin is NodeOutputTypePin otp && otp.OutgoingPins.Count > 0))
                {
                    return BorderBrush;
                }

                var brush = (SolidColorBrush)BorderBrush;
                var color = Color.FromArgb(brush.Color.A, (byte)(brush.Color.R * 0.6), (byte)(brush.Color.G * 0.6), (byte)(brush.Color.B * 0.6));
                return new SolidColorBrush(color);
            }
        }

        public bool ShowRectangle
        {
            get => pin is NodeExecPin;
        }

        public bool ShowCircle
        {
            get => pin is NodeDataPin;
        }

        public bool ShowTriangle
        {
            get => pin is NodeTypePin;
        }

        public Point AbsolutePosition
        {
            // Node abs + Node->PinCenter
            get => new Point(
                Node.PositionX + NodeRelativePosition.X , 
                Node.PositionY + NodeRelativePosition.Y);
        }

        public Point NodeRelativePosition
        {
            get => nodeRelativePosition;
            set
            {
                if(nodeRelativePosition != value)
                {
                    nodeRelativePosition = value;
                    OnConnectionPositionUpdate();
                }
            }
        }

        private Point nodeRelativePosition;

        private void OnConnectionPositionUpdate()
        {
            OnPropertyChanged(nameof(NodeRelativePosition));
            OnPropertyChanged(nameof(ConnectedAbsolutePosition));
            OnPropertyChanged(nameof(ConnectedCP1));
            OnPropertyChanged(nameof(ConnectedCP2));
            OnPropertyChanged(nameof(AbsolutePosition));
        }

        // = Incoming pin for data input
        // = Outgoing pin for exec output
        // = Incoming pin for type input
        // = null for rest
        public NodePinVM ConnectedPin
        {
            get => connectedPin;
            set
            {
                if(!(Pin is NodeOutputExecPin || Pin is NodeInputDataPin || Pin is NodeInputTypePin))
                {
                    throw new Exception("Can only set connected pin of NodeOutputExecPin and NodeInputDataPin");
                }

                if(connectedPin != value)
                {
                    if (connectedPin != null)
                    {
                        connectedPin.Node.OnPositionChanged -= OnConnectedPinNodePositionChanged;
                        connectedPin.PropertyChanged -= OnConnectedPinPropertyChanged;
                    }

                    if (value != null)
                    {
                        GraphUtil.ConnectNodePins(Pin, value.Pin);
                    }
                    else
                    {
                        if (Pin is NodeOutputExecPin oxp)
                        {
                            GraphUtil.DisconnectOutputExecPin(oxp);
                        }
                        else if (Pin is NodeInputDataPin idp)
                        {
                            GraphUtil.DisconnectInputDataPin(idp);
                        }
                        else if (Pin is NodeInputTypePin itp)
                        {
                            GraphUtil.DisconnectInputTypePin(itp);
                        }
                    }

                    connectedPin = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsConnected));
                    OnPropertyChanged(nameof(IsCableVisible));
                    OnPropertyChanged(nameof(ShowUnconnectedValue));
                    OnPropertyChanged(nameof(ShowEnumValue));
                    OnPropertyChanged(nameof(PossibleEnumNames));
                    OnPropertyChanged(nameof(FillBrush));
                    OnConnectionPositionUpdate();

                    if (connectedPin != null)
                    {
                        connectedPin.Node.OnPositionChanged += OnConnectedPinNodePositionChanged;
                        connectedPin.PropertyChanged += OnConnectedPinPropertyChanged;
                    }
                }
            }
        }

        private void OnConnectedPinPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Position) || e.PropertyName == nameof(NodeRelativePosition))
            {
                OnConnectionPositionUpdate();
            }
        }

        public bool IsConnected
        {
            get => connectedPin != null;
        }

        public bool IsCableVisible
        {
            get => IsConnected || IsBeingConnected;
        }

        private void OnConnectedPinNodePositionChanged(Node node, double posX, double posY)
        {
            OnConnectionPositionUpdate();
        }

        private const double CPOffset = 100;

        public Point ConnectedCP1
        {
            get
            {
                if (Pin is NodeOutputExecPin || Pin is NodeOutputDataPin || Pin is NodeOutputTypePin)
                {
                    return new Point(AbsolutePosition.X + CPOffset, AbsolutePosition.Y);
                }
                else
                {
                    return new Point(AbsolutePosition.X - CPOffset, AbsolutePosition.Y);
                }
            }
        }

        public Point ConnectedCP2
        {
            get
            {
                if (Pin is NodeOutputExecPin || Pin is NodeOutputDataPin || Pin is NodeOutputTypePin)
                {
                    return new Point(ConnectedAbsolutePosition.X - CPOffset, ConnectedAbsolutePosition.Y);
                }
                else
                {
                    return new Point(ConnectedAbsolutePosition.X + CPOffset, ConnectedAbsolutePosition.Y);
                }
            }
        }

        public Point ConnectedAbsolutePosition
        {
            get
            {
                if (IsBeingConnected)
                {
                    return ConnectingAbsolutePosition;
                }
                else
                {
                    return IsConnected ? ConnectedPin.AbsolutePosition : AbsolutePosition;
                }
            }
        }

        private NodePin pin;
        private NodePinVM connectedPin;

        private double positionX;
        private double positionY;

        public NodePinVM(NodePin pin)
        {
            Pin = pin;
        }

        /// <summary>
        /// Adds a reroute node for this pin. Only valid
        /// for input data pins and output execution pins.
        /// </summary>
        public void AddRerouteNode()
        {
            if (Pin is NodeInputDataPin dataPin)
            {
                RerouteNode rerouteNode = GraphUtil.AddRerouteNode(dataPin);
                rerouteNode.PositionX = (Pin.Node.PositionX + dataPin.IncomingPin.Node.PositionX) / 2;
                rerouteNode.PositionY = (Pin.Node.PositionY + dataPin.IncomingPin.Node.PositionY) / 2;
            }
            else if (Pin is NodeOutputExecPin execPin)
            {
                RerouteNode rerouteNode = GraphUtil.AddRerouteNode(execPin);
                rerouteNode.PositionX = (Pin.Node.PositionX + execPin.OutgoingPin.Node.PositionX) / 2;
                rerouteNode.PositionY = (Pin.Node.PositionY + execPin.OutgoingPin.Node.PositionY) / 2;
            }
            else if (Pin is NodeInputTypePin typePin)
            {
                RerouteNode rerouteNode = GraphUtil.AddRerouteNode(typePin);
                rerouteNode.PositionX = (Pin.Node.PositionX + typePin.IncomingPin.Node.PositionX) / 2;
                rerouteNode.PositionY = (Pin.Node.PositionY + typePin.IncomingPin.Node.PositionY) / 2;
            }
            else
            {
                throw new Exception("Can't add reroute node for invalid pin type");
            }
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
