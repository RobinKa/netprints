using NetPrints.Core;
using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    public class VariableGetterNode : VariableNode
    {
        public VariableGetterNode(Method method, TypeSpecifier targetType, string variableName, BaseType variableType) 
            : base(method, targetType, variableName, variableType)
        {
        }

        public override string ToString()
        {
            return $"Get {VariableName}";
        }
    }
}
