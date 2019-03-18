using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Abstract base class for node pins.
    /// </summary>
    [DataContract]
    [KnownType(typeof(NodeInputDataPin))]
    [KnownType(typeof(NodeOutputDataPin))]
    [KnownType(typeof(NodeInputExecPin))]
    [KnownType(typeof(NodeOutputExecPin))]
    [KnownType(typeof(NodeInputTypePin))]
    [KnownType(typeof(NodeOutputTypePin))]
    public abstract class NodePin
    {
        /// <summary>
        /// Name of the pin.
        /// </summary>
        [DataMember]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Node this pin is contained in.
        /// </summary>
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
