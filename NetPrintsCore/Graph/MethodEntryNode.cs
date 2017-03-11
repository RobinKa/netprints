using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public class MethodEntryNode : Node
    {
        public MethodEntryNode(IEnumerable<Type> parameterTypes)
        {
            AddOutputExecPin("Exec");

            foreach(Type paramType in parameterTypes)
            {
                AddOutputDataPin(paramType.Name, paramType);
            }
        }
    }
}
