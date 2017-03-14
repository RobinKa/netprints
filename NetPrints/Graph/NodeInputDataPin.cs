using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public class NodeInputDataPin : NodeDataPin
    {
        public NodeOutputDataPin IncomingPin
        {
            get;
            set;
        }

        public NodeInputDataPin(Node node, string name, Type pinType)
            : base(node, name, pinType)
        {
            
        }
    }
}
