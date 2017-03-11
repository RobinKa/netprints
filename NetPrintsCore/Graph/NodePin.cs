using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public abstract class NodePin
    {
        public string Name
        {
            get;
            set;
        }

        public Node Node
        {
            get;
            private set;
        }

        public NodePin(Node node, string name)
        {
            Node = node;
            Name = name;
        }
    }
}
