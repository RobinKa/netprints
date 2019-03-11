using NetPrints.Core;
using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Abstract class for variable nodes.
    /// </summary>
    [DataContract]
    [KnownType(typeof(VariableGetterNode))]
    [KnownType(typeof(VariableSetterNode))]
    public abstract class VariableNode : Node
    {
        /// <summary>
        /// Target object of this variable node.
        /// Can be null for local variables.
        /// </summary>
        public NodeInputDataPin TargetPin
        {
            get { return InputDataPins[0]; }
        }

        /// <summary>
        /// Pin that outputs the value of the variable.
        /// </summary>
        public NodeOutputDataPin ValuePin
        {
            get { return OutputDataPins[0]; }
        }

        /// <summary>
        /// Whether the variable is a local variable.
        /// </summary>
        public bool IsLocalVariable => TargetType.Equals(null);

        /// <summary>
        /// Name of this variable.
        /// </summary>
        [DataMember]
        public string VariableName { get; private set; }

        /// <summary>
        /// Specifier for the type of the target object.
        /// </summary>
        [DataMember]
        public TypeSpecifier TargetType { get; private set; }

        public VariableNode(Method method, TypeSpecifier targetType, string variableName, BaseType variableType)
            : base(method)
        {
            VariableName = variableName;
            TargetType = targetType;

            // TargetType null means local variable

            if (!targetType.Equals(null))
            {
                AddInputDataPin("Target", targetType);
            }

            AddOutputDataPin(variableType.ShortName, variableType);
        }
    }
}
