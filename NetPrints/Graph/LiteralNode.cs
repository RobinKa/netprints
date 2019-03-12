using NetPrints.Core;
using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Node representing a literal value.
    /// </summary>
    [DataContract]
    public class LiteralNode : Node
    {
        /// <summary>
        /// Output data pin for the value of this literal.
        /// </summary>
        public NodeOutputDataPin ValuePin
        {
            get { return OutputDataPins[0]; }
        }

        /// <summary>
        /// Specifier for the type of this literal.
        /// </summary>
        [DataMember]
        public TypeSpecifier LiteralType { get; private set; }

        /// <summary>
        /// Value of the literal.
        /// </summary>
        [DataMember]
        public object Value
        {
            get
            {
                return val;
            }
            set
            {
                if (TypeSpecifier.FromType(value.GetType()) != LiteralType)
                {
                    throw new ArgumentException("Value is not of the same type as LiteralType");
                }

                val = value;
            }
        }

        private object val;

        public LiteralNode(Method method, TypeSpecifier literalType, object value)
            : base(method)
        {
            LiteralType = literalType;
            Value = value;

            AddOutputDataPin("Value", literalType);
        }

        public override string ToString()
        {
            return $"{LiteralType.Name}";
        }
    }
}
