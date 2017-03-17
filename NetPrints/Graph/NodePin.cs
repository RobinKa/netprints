using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
