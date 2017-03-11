using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public class NodeOutputDataPin : NodeDataPin
    {
        public IList<NodeInputDataPin> OutgoingPins { get; } = new List<NodeInputDataPin>();

        public NodeOutputDataPin(Node node, string name, Type pinType)
            : base(node, name, pinType)
        {

        }
    }
}
