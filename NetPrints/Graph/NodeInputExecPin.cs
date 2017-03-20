using NetPrints.Core;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    public class NodeInputExecPin : NodeExecPin
    {
        [DataMember]
        public ObservableRangeCollection<NodeOutputExecPin> IncomingPins { get; private set; } = 
            new ObservableRangeCollection<NodeOutputExecPin>();

        public NodeInputExecPin(Node node, string name)
            : base(node, name)
        {
            
        }
    }
}
