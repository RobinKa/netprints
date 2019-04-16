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
        public ObservableValue<BaseType> PinType { get; private set; }

        protected NodeDataPin(Node node, string name, ObservableValue<BaseType> pinType)
            : base(node, name)
        {
            PinType = pinType;
        }

        public override string ToString()
        {
            return $"{Name}: {PinType.Value.ShortName}";
        }
    }
}
