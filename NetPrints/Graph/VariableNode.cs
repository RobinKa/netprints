using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public class VariableNode : Node
    {
        public NodeInputDataPin TargetPin
        {
            get { return InputDataPins[0]; }
        }

        public NodeOutputDataPin ValuePin
        {
            get { return OutputDataPins[0]; }
        }

        public string VariableName { get; private set; }

        public VariableNode(Method method, string variableName, Type variableType)
            : base(method)
        {
            VariableName = variableName;
            
            AddInputDataPin("Target", typeof(object));
            AddOutputDataPin(variableType.Name, variableType);
        }
    }
}
