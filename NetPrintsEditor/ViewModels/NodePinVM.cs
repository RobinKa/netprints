using NetPrints.Graph;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace NetPrintsEditor.ViewModels
{
    public class NodePinVM : INotifyPropertyChanged
    {
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

                    pin.Node.OnPositionChanged += OnNodePositionChanged;

                    OnPropertyChanged();
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
                if (Pin is NodeInputDataPin p && p.UnconnectedValue != value)
                {
                    p.UnconnectedValue = Convert.ChangeType(value, p.PinType);
                    OnPropertyChanged();
                }
            }
        }

        public bool IsUsingUnconnectedValue
        {
            get => Pin is NodeInputDataPin p && p.IncomingPin == null && 
                (p.PinType == typeof(string) ||
                p.PinType == typeof(int));
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
                    OnConnectionPositionUpdate();
                }
            }
        }

        public Point Position
        {
            get => new Point(PositionX, PositionY);
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
            OnPropertyChanged(nameof(ConnectedPositionY));
            OnPropertyChanged(nameof(ConnectedPositionY));
            OnPropertyChanged(nameof(ConnectedPosition));
            OnPropertyChanged(nameof(ConnectedCP1));
            OnPropertyChanged(nameof(ConnectedCP2));
        }

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
                    OnPropertyChanged(nameof(IsUsingUnconnectedValue));
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

        private void OnConnectedPinNodePositionChanged(Node node, double posX, double posY)
        {
            OnConnectionPositionUpdate();
        }

        public double ConnectedPositionX
        {
            get
            {
                if (connectedPin != null)
                {
                    return connectedPin.NodeRelativePosition.X - NodeRelativePosition.X + 
                        connectedPin.Node.PositionX - Node.PositionX
                        + connectedPin.PositionX;
                }
                else
                {
                    return PositionX;
                }
            }
        }

        public double ConnectedPositionY
        {
            get
            {
                if (connectedPin != null)
                {
                    return connectedPin.NodeRelativePosition.Y - NodeRelativePosition.Y +
                        connectedPin.Node.PositionY - Node.PositionY
                        + connectedPin.PositionY;
                }
                else
                {
                    return PositionY;
                }
            }
        }

        private const double CPOffset = 100;

        public Point ConnectedCP1
        {
            get
            {
                if (Pin is NodeOutputExecPin)
                {
                    return new Point(PositionX + CPOffset, PositionY);
                }
                else
                {
                    return new Point(PositionX - CPOffset, PositionY);
                }
            }
        }

        public Point ConnectedCP2
        {
            get
            {
                if (Pin is NodeOutputExecPin)
                {
                    return new Point(ConnectedPositionX - CPOffset, ConnectedPositionY);
                }
                else
                {
                    return new Point(ConnectedPositionX + CPOffset, ConnectedPositionY);
                }
            }
        }

        public Point ConnectedPosition
        {
            get => new Point(ConnectedPositionX, ConnectedPositionY);
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
