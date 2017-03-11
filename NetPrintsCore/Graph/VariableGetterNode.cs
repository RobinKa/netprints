using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public class VariableGetterNode : VariableNode
    {
        public VariableGetterNode(string variableName, Type variableType) 
            : base(variableName, variableType)
        {
        }
    }
}
