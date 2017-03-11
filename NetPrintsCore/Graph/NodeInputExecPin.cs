using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public class NodeInputExecPin : NodeExecPin
    {
        public IList<NodeInputExecPin> IncomingPins { get; } = new List<NodeInputExecPin>();

        public NodeInputExecPin(Node node, string name)
            : base(node, name)
        {
            
        }
    }
}
