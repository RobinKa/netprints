using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public class VariableGetterNode : VariableNode
    {
        public VariableGetterNode(Method method, string variableName, Type variableType) 
            : base(method, variableName, variableType)
        {
        }

        public override string ToString()
        {
            return $"Get {VariableName}";
        }
    }
}
