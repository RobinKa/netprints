using NetPrints.Core;
using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    public abstract class NodeDataPin : NodePin
    {
        [DataMember]
        public BaseType PinType { get; private set; }

        public NodeDataPin(Node node, string name, BaseType pinType)
            : base(node, name)
        {
            PinType = pinType;
        }
    }
}
