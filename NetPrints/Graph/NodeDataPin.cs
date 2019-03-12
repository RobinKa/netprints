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
        /// <summary>
        /// Specifier for the type of this data pin.
        /// </summary>
        [DataMember]
        public BaseType PinType { get; private set; }

        public NodeDataPin(Node node, string name, BaseType pinType)
            : base(node, name)
        {
            PinType = pinType;
        }
    }
}
