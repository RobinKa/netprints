using NetPrints.Base;
using NetPrints.Core;
using PropertyChanged;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Abstract base class for node pins.
    /// </summary>
    [DataContract]
    [KnownType(typeof(NodeInputDataPin))]
    [KnownType(typeof(NodeOutputDataPin))]
    [KnownType(typeof(NodeInputExecPin))]
    [KnownType(typeof(NodeOutputExecPin))]
    [KnownType(typeof(NodeInputTypePin))]
    [KnownType(typeof(NodeOutputTypePin))]
    [AddINotifyPropertyChangedInterface]
    public abstract class NodePin : INodePin
    {
        private double positionX;
        private double positionY;

        /// <summary>
        /// Name of the pin.
        /// </summary>
        [DataMember]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Node this pin is contained in.
        /// </summary>
        [DataMember]
        public Node Node
        {
            get;
            private set;
        }

        public double PositionX
        {
            get { return positionX; }
            set { positionX = value; PositionChanged?.Invoke(this, EventArgs.Empty); }
        }
        public double PositionY
        {
            get { return positionY; }
            set { positionY = value; PositionChanged?.Invoke(this, EventArgs.Empty); }
        }

        public event EventHandler PositionChanged;

        INode INodePin.Node => Node;

        public abstract NodePinConnectionType ConnectionType { get; }

        public ObservableRangeCollection<INodePin> ConnectedPins => connectedPins;

        [DataMember]
        private ObservableRangeCollection<INodePin> connectedPins = new ObservableRangeCollection<INodePin>();

        protected NodePin(Node node, string name)
        {
            Node = node;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
