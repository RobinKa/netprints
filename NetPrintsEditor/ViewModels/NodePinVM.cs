using NetPrints.Core;
using NetPrints.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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
                if(Pin is NodeDataPin dataPin)
                {
                    if (dataPin.Node is CallMethodNode callMethodNode)
                    {
                        if (dataPin is NodeInputDataPin inputDataPin)
                        {
                            int paramIndex = callMethodNode.ArgumentPins.IndexOf(inputDataPin);
                            if (paramIndex >= 0)
                            {
                                return ProjectVM.Instance.ReflectionProvider.GetMethodParameterDocumentation(callMethodNode.MethodSpecifier, paramIndex);
                            }
                        }
                        else if(dataPin is NodeOutputDataPin outputDataPin)
                        {
                            int returnIndex = callMethodNode.OutputDataPins.IndexOf(outputDataPin);
                            if (returnIndex >= 0)
                            {
                                return ProjectVM.Instance.ReflectionProvider.GetMethodReturnDocumentation(callMethodNode.MethodSpecifier, returnIndex);
                            }
                        }
                    }

                    return $"{dataPin.PinType} {dataPin.Name}";
                }

                return null;
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
                    // and is an IDP or OXP
                    if(value && (Pin is NodeInputDataPin || Pin is NodeOutputExecPin))
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
                    }

                    pin = value;

                    if (pin != null)
                    {
                        pin.Node.OnPositionChanged += OnNodePositionChanged;
                    }

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Brush));
                    OnPropertyChanged(nameof(ShowUnconnectedValue));
                    OnPropertyChanged(nameof(ShowEnumValue));
                    OnPropertyChanged(nameof(PossibleEnumNames));
                    OnPropertyChanged(nameof(ToolTip));
                }
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
                    && p.PinType is TypeSpecifier typeSpecifier)
                {
                    // Try to convert to the correct type first if it can be found
                    // Dont do this for enums as they just use a string

                    Type t = Type.GetType(typeSpecifier.Name);
                    
                    if (t != null && !typeSpecifier.IsEnum)
                    {
                        p.UnconnectedValue = Convert.ChangeType(value, t);
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
                !(p.PinType is TypeSpecifier typeSpec && typeSpec.IsEnum);
        }

        public bool ShowEnumValue
        {
            get => Pin is NodeInputDataPin p && p.UsesUnconnectedValue && !IsConnected && 
                (p.PinType is TypeSpecifier typeSpec && typeSpec.IsEnum);
        }

        public IEnumerable<string> PossibleEnumNames
        {
            get
            {
                if (Pin is NodeInputDataPin p && p.PinType is TypeSpecifier typeSpec && typeSpec.IsEnum)
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

        public string Name
        {
            get => pin.Name;
            set
            {
                if(pin.Name != value)
                {
                    pin.Name = value;
                    OnPropertyChanged();
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

        private static readonly SolidColorBrush ExecPinBrush =
            new SolidColorBrush(Color.FromArgb(0xFF, 0xE0, 0xFF, 0xE0));

        private static readonly SolidColorBrush DataPinBrush =
            new SolidColorBrush(Color.FromArgb(0xFF, 0xE0, 0xE0, 0xFF));

        public Brush Brush
        {
            get => (Pin is NodeDataPin) ? DataPinBrush : ExecPinBrush;
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
        // = null for rest
        public NodePinVM ConnectedPin
        {
            get => connectedPin;
            set
            {
                if(!(Pin is NodeOutputExecPin || Pin is NodeInputDataPin))
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
                        if(Pin is NodeOutputExecPin oxp)
                        {
                            GraphUtil.DisconnectOutputExecPin(oxp);
                        }
                        else if(Pin is NodeInputDataPin idp)
                        {
                            GraphUtil.DisconnectInputDataPin(idp);
                        }
                    }

                    connectedPin = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsConnected));
                    OnPropertyChanged(nameof(IsCableVisible));
                    OnPropertyChanged(nameof(ShowUnconnectedValue));
                    OnPropertyChanged(nameof(ShowEnumValue));
                    OnPropertyChanged(nameof(PossibleEnumNames));
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
                if (Pin is NodeOutputExecPin || Pin is NodeOutputDataPin)
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
                if (Pin is NodeOutputExecPin || Pin is NodeOutputDataPin)
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

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
