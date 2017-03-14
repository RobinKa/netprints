using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public class NodeInputExecPin : NodeExecPin
    {
        public IList<NodeOutputExecPin> IncomingPins { get; } = new List<NodeOutputExecPin>();

        public NodeInputExecPin(Node node, string name)
            : base(node, name)
        {
            
        }
    }
}
