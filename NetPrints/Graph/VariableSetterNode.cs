using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public class VariableSetterNode : VariableNode
    {
        public NodeInputDataPin NewValuePin
        {
            get { return InputDataPins[1]; }
        }

        public VariableSetterNode(Method method, string variableName, Type variableType) 
            : base(method, variableName, variableType)
        {
            AddInputExecPin("Exec");
            AddOutputExecPin("Exec");

            AddInputDataPin("NewValue", variableType);
        }
    }
}
