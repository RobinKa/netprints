using NetPrints.Core;
using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Node representing a literal value.
    /// </summary>
    [DataContract]
    public class TypeOfNode : Node
    {
        /// <summary>
        /// Output data pin for the Type value.
        /// </summary>
        public NodeOutputDataPin TypePin
        {
            get { return OutputDataPins[0]; }
        }

        /// <summary>
        /// Specifier for the type of this typeof node.
        /// </summary>
        [DataMember]
        public TypeSpecifier Type { get; private set; }

        public TypeOfNode(Method method, TypeSpecifier type)
            : base(method)
        {
            Type = type;

            AddOutputDataPin("Type", TypeSpecifier.FromType<Type>());
        }

        public override string ToString()
        {
            return $"Type of {Type.Name}";
        }
    }
}
