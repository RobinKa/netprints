using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    [DataContract]
    public class NodeExecPin : NodePin
    {
        public NodeExecPin(Node node, string name)
            : base(node, name)
        {

        }
    }
}
