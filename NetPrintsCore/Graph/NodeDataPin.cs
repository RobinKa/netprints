using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public abstract class NodeDataPin : NodePin
    {
        public Type PinType { get; private set; }

        public NodeDataPin(Node node, string name, Type pinType)
            : base(node, name)
        {
            PinType = pinType;
        }
    }
}
