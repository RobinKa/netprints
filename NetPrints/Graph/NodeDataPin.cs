using NetPrints.Core;
using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    public abstract class NodeDataPin : NodePin
    {
        [DataMember]
        public TypeSpecifier PinType { get; private set; }

        public NodeDataPin(Node node, string name, TypeSpecifier pinType)
            : base(node, name)
        {
            PinType = pinType;
        }
    }
}
