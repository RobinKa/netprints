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
        /// Input type pin for the Type value.
        /// </summary>
        public NodeInputTypePin InputTypePin
        {
            get { return InputTypePins[0]; }
        }

        public TypeOfNode(Method method)
            : base(method)
        {
            AddInputTypePin("Type");
            AddOutputDataPin("Type", TypeSpecifier.FromType<Type>());
        }

        public override string ToString()
        {
            return $"Type Of";
        }
    }
}
