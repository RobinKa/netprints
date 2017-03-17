using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    [DataContract]
    public abstract class NodeDataPin : NodePin
    {
        [DataMember]
        public Type PinType { get; private set; }

        public NodeDataPin(Node node, string name, Type pinType)
            : base(node, name)
        {
            PinType = pinType;
        }
    }
}
