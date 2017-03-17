using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    [DataContract]
    [KnownType(typeof(CallMethodNode))]
    [KnownType(typeof(CallStaticFunctionNode))]
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
