using NetPrints.Core;
using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Node that gets the value of a variable.
    /// </summary>
    [DataContract]
    public class VariableGetterNode : VariableNode
    {
        public VariableGetterNode(Method method, TypeSpecifier targetType, Variable variable) 
            : base(method, targetType, variable)
        {
        }

        public override string ToString()
        {
            string staticText = IsStatic ? "Static " : "";
            return $"Get {staticText}{VariableName}";
        }
    }
}
