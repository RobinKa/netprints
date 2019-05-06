using NetPrints.Core;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    public class TypeReturnNode : Node
    {
        public NodeInputTypePin TypePin => InputTypePins[0];

        public TypeReturnNode(TypeGraph graph)
            : base(graph)
        {
            AddInputTypePin("Type");
        }
    }
}
