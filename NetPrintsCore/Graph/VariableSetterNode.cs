using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public class VariableSetterNode : VariableNode
    {
        public VariableSetterNode(string variableName, Type variableType) 
            : base(variableName, variableType)
        {
            AddInputExecPin("Exec");
            AddOutputExecPin("Exec");

            AddInputDataPin("NewValue", variableType);
        }
    }
}
