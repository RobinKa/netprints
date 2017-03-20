using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    [KnownType(typeof(NodeInputDataPin))]
    [KnownType(typeof(NodeOutputDataPin))]
    [KnownType(typeof(NodeInputExecPin))]
    [KnownType(typeof(NodeOutputExecPin))]
    public abstract class NodePin
    {
        [DataMember]
        public string Name
        {
            get;
            set;
        }

        [DataMember]
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
