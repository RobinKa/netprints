using GalaSoft.MvvmLight;
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
    public class NodePinVM : ViewModelBase
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
                                documentation = App.ReflectionProvider.GetMethodParameterDocumentation(callMethodNode.MethodSpecifier, paramIndex);
                            }
                        }
                        else if (dataPin is NodeOutputDataPin outputDataPin)
                        {
                            int returnIndex = callMethodNode.OutputDataPins.IndexOf(outputDataPin);
                            if (returnIndex >= 0)
                            {
                                documentation = App.ReflectionProvider.GetMethodReturnDocumentation(callMethodNode.MethodSpecifier, returnIndex);
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
                if (connectingRelativeMousePosition != value)
                {
                    connectingRelativeMousePosition = value;
                    RaisePropertyChanged();
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
                if (isBeingConnected != value)
                {
                    isBeingConnected = value;

                    // Disconnect pin if its being connected
                    // and is an IDP, OXP or ITP
                    if (value && (Pin is NodeInputDataPin || Pin is NodeOutputExecPin || Pin is NodeInputTypePin))
                    {
                        ConnectedPin = null;
                    }

                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsCableVisible));
                    OnConnectionPositionUpdate();
                }
            }
        }

        private bool isBeingConnected = false;

        // TODO: Save this property (perhaps move to model)
        public bool IsFaint { get; set; }

        public NodePin Pin
        {
            get => pin;
            set
            {
                if (pin != value)
                {
                    if (pin != null)
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

                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(FillBrush));
                    RaisePropertyChanged(nameof(BorderBrush));
                    RaisePropertyChanged(nameof(ShowUnconnectedValue));
                    RaisePropertyChanged(nameof(ShowEnumValue));
                    RaisePropertyChanged(nameof(ShowBooleanValue));
                    RaisePropertyChanged(nameof(PossibleEnumNames));
                    RaisePropertyChanged(nameof(ToolTip));
                    RaisePropertyChanged(nameof(IsRerouteNodePin));
                }
            }
        }

        private void OnInputTypeChanged(object sender, EventArgs eventArgs)
        {
            RaisePropertyChanged(nameof(PossibleEnumNames));
            RaisePropertyChanged(nameof(ShowUnconnectedValue));
            RaisePropertyChanged(nameof(ShowBooleanValue));
            RaisePropertyChanged(nameof(ShowEnumValue));
            RaisePropertyChanged(nameof(DisplayName));
            RaisePropertyChanged(nameof(ToolTip));
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

        public object UnconnectedValue
        {
            get
            {
                if (Pin is NodeInputDataPin p)
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

                    RaisePropertyChanged();
                }
            }
        }

        public bool ShowUnconnectedValue
        {
            get => Pin is NodeInputDataPin p && p.UsesUnconnectedValue && !IsConnected
                && !(p.PinType.Value is TypeSpecifier typeSpec && (typeSpec.IsEnum || typeSpec == TypeSpecifier.FromType<bool>()));
        }

        public bool ShowEnumValue
        {
            get => Pin is NodeInputDataPin p && p.UsesUnconnectedValue && !IsConnected
                && (p.PinType.Value is TypeSpecifier typeSpec && typeSpec.IsEnum);
        }

        public bool ShowBooleanValue
        {
            get => Pin is NodeInputDataPin p && p.UsesUnconnectedValue && !IsConnected
                && p.PinType.Value is TypeSpecifier typeSpec
                && typeSpec == TypeSpecifier.FromType<bool>();
        }

        public IEnumerable<string> PossibleEnumNames
        {
            get
            {
                if (Pin is NodeInputDataPin p && p.PinType.Value is TypeSpecifier typeSpec && typeSpec.IsEnum)
                {
                    return App.ReflectionProvider.GetEnumNames(typeSpec);
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
            get => (pin.Node is MethodEntryNode && pin.Node.OutputDataPins.Contains(pin))
                || (pin.Node is ReturnNode && pin.Node.InputDataPins.Contains(pin));
        }

        public string Name
        {
            get => pin.Name;
            set
            {
                if (pin.Name != value)
                {
                    pin.Name = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(DisplayName));
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
                if (positionX != value)
                {
                    positionX = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Position));
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
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(Position));
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
                if ((pin is NodeInputDataPin idp && idp.IncomingPin != null)
                    || (pin is NodeOutputDataPin odp && odp.OutgoingPins.Count > 0)
                    || (pin is NodeInputExecPin iep && iep.IncomingPins.Count > 0)
                    || (pin is NodeOutputExecPin oep && oep.OutgoingPin != null)
                    || (pin is NodeInputTypePin itp && itp.IncomingPin != null)
                    || (pin is NodeOutputTypePin otp && otp.OutgoingPins.Count > 0))
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
                Node.PositionX + NodeRelativePosition.X,
                Node.PositionY + NodeRelativePosition.Y);
        }

        public Point NodeRelativePosition
        {
            get => nodeRelativePosition;
            set
            {
                if (nodeRelativePosition != value)
                {
                    nodeRelativePosition = value;
                    OnConnectionPositionUpdate();
                }
            }
        }

        private Point nodeRelativePosition;

        private void OnConnectionPositionUpdate()
        {
            RaisePropertyChanged(nameof(NodeRelativePosition));
            RaisePropertyChanged(nameof(ConnectedAbsolutePosition));
            RaisePropertyChanged(nameof(ConnectedCP1));
            RaisePropertyChanged(nameof(ConnectedCP2));
            RaisePropertyChanged(nameof(AbsolutePosition));
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
                if (!(Pin is NodeOutputExecPin || Pin is NodeInputDataPin || Pin is NodeInputTypePin))
                {
                    throw new Exception("Can only set connected pin of NodeOutputExecPin and NodeInputDataPin");
                }

                if (connectedPin != value)
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
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsConnected));
                    RaisePropertyChanged(nameof(IsCableVisible));
                    RaisePropertyChanged(nameof(ShowUnconnectedValue));
                    RaisePropertyChanged(nameof(ShowEnumValue));
                    RaisePropertyChanged(nameof(PossibleEnumNames));
                    RaisePropertyChanged(nameof(FillBrush));
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
    }
}
