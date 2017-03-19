using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    [DataContract]
    public class NodeInputExecPin : NodeExecPin
    {
        [DataMember]
        public List<NodeOutputExecPin> IncomingPins { get; private set; } = new List<NodeOutputExecPin>();

        public NodeInputExecPin(Node node, string name)
            : base(node, name)
        {
            
        }
    }
}
