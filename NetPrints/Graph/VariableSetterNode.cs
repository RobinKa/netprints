using NetPrints.Core;
using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    public class VariableSetterNode : VariableNode
    {
        public NodeInputDataPin NewValuePin
        {
            get { return InputDataPins[1]; }
        }

        public VariableSetterNode(Method method, TypeSpecifier targetType, string variableName, TypeSpecifier variableType) 
            : base(method, targetType, variableName, variableType)
        {
            AddInputExecPin("Exec");
            AddOutputExecPin("Exec");

            AddInputDataPin("NewValue", variableType);
        }

        public override string ToString()
        {
            return $"Set {VariableName}";
        }
    }
}
