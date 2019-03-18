using NetPrints.Core;
using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Abstract class for data pins.
    /// </summary>
    [DataContract]
    public abstract class NodeDataPin : NodePin
    {
        // TODO: Add pin type changed event

        /// <summary>
        /// Specifier for the type of this data pin.
        /// </summary>
        [DataMember]
        public BaseType PinType { get; set; }

        public NodeDataPin(Node node, string name, BaseType pinType)
            : base(node, name)
        {
            PinType = pinType;
        }
    }
}
