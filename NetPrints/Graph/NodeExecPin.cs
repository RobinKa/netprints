using System.Runtime.Serialization;

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
