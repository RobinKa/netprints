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

        [DataMember]
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
