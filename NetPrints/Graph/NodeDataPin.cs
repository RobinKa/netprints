using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    public abstract class NodeDataPin : NodePin
    {
        [DataMember]
        public Type PinType { get; private set; }

        public NodeDataPin(Node node, string name, Type pinType)
            : base(node, name)
        {
            PinType = pinType;
        }
    }
}
