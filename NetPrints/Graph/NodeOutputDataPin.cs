using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    [DataContract]
    public class NodeOutputDataPin : NodeDataPin
    {
        [DataMember]
        public IList<NodeInputDataPin> OutgoingPins { get; private set; } = new List<NodeInputDataPin>();

        public NodeOutputDataPin(Node node, string name, Type pinType)
            : base(node, name, pinType)
        {

        }
    }
}
