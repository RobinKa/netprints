using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
