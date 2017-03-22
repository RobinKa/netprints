using NetPrints.Core;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    [KnownType(typeof(CallMethodNode))]
    [KnownType(typeof(ConstructorNode))]
    public abstract class ExecNode : Node
    {
        public ExecNode(Method method)
            : base(method)
        {
            AddInputExecPin("Exec");
            AddOutputExecPin("Exec");
        }
    }
}
