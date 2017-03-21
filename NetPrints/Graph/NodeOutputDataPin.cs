using NetPrints.Core;
using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    public class NodeOutputDataPin : NodeDataPin
    {
        [DataMember]
        public ObservableRangeCollection<NodeInputDataPin> OutgoingPins { get; private set; } 
            = new ObservableRangeCollection<NodeInputDataPin>();

        public NodeOutputDataPin(Node node, string name, TypeSpecifier pinType)
            : base(node, name, pinType)
        {

        }
    }
}
