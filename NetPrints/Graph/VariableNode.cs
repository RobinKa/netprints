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

        [DataMember]
        public string VariableName { get; private set; }

        public VariableNode(Method method, string variableName, TypeSpecifier variableType)
            : base(method)
        {
            VariableName = variableName;
            
            AddInputDataPin("Target", typeof(object));
            AddOutputDataPin(variableType.ShortName, variableType);
        }
    }
}
