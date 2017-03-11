using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public class VariableNode : Node
    {
        public string VariableName { get; private set; }

        public VariableNode(string variableName, Type variableType)
        {
            VariableName = variableName;
            
            AddInputDataPin("Target", typeof(object));
            AddOutputDataPin(variableType.Name, variableType);
        }
    }
}
