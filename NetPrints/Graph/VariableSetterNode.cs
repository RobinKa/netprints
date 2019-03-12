using NetPrints.Core;
using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Node that sets the value of a variable.
    /// </summary>
    [DataContract]
    public class VariableSetterNode : VariableNode
    {
        /// <summary>
        /// Input data pin for the new value of the variable.
        /// </summary>
        public NodeInputDataPin NewValuePin
        {
            get { return InputDataPins[1]; }
        }

        public VariableSetterNode(Method method, TypeSpecifier targetType, string variableName, BaseType variableType) 
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
