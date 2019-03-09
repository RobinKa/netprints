using NetPrints.Core;
using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    [KnownType(typeof(VariableGetterNode))]
    [KnownType(typeof(VariableSetterNode))]
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

        public bool IsLocalVariable => TargetType.Equals(null);

        [DataMember]
        public string VariableName { get; private set; }

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
