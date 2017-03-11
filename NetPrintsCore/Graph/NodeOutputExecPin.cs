using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public class NodeOutputExecPin : NodeExecPin
    {
        public NodeInputExecPin OutgoingPin { get; set; }

        public NodeOutputExecPin(Node node, string name)
            : base(node, name)
        {
            
        }
    }
}
