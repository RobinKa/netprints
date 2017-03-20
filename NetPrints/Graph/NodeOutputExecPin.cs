using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    public class NodeOutputExecPin : NodeExecPin
    {
        [DataMember]
        public NodeInputExecPin OutgoingPin { get; set; }

        public NodeOutputExecPin(Node node, string name)
            : base(node, name)
        {
            
        }
    }
}
