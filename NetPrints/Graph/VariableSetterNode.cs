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
            get { return IsStatic ? InputDataPins[0] : InputDataPins[1]; }
        }

        public VariableSetterNode(Method method, VariableSpecifier variable)
            : base(method, variable)
        {
            AddInputExecPin("Exec");
            AddOutputExecPin("Exec");

            AddInputDataPin("NewValue", variable.Type);
        }

        public override string ToString()
        {
            string staticText = IsStatic ? $"{TargetType.ShortName}." : "";
            return $"Set {staticText}{VariableName}";
        }
    }
}
