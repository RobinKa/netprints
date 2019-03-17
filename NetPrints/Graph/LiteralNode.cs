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
        /// Input data pin for the value of this literal.
        /// </summary>
        public NodeInputDataPin InputValuePin
        {
            get { return InputDataPins[0]; }
        }

        /// <summary>
        /// Specifier for the type of this literal.
        /// </summary>
        [DataMember]
        public TypeSpecifier LiteralType { get; private set; }

        public LiteralNode(Method method, TypeSpecifier literalType)
            : base(method)
        {
            LiteralType = literalType;

            AddInputDataPin("Value", literalType);
            AddOutputDataPin("Value", literalType);
        }

        /// <summary>
        /// Creates a literal node and gives it an unconnected value.
        /// </summary>
        /// <typeparam name="T">Type of the unconnected value.</typeparam>
        /// <param name="val">Value when the input pin is unconnected.</param>
        /// <returns>Literal node with the specified unconnected value.</returns>
        public static LiteralNode WithValue<T>(Method method, T val)
        {
            LiteralNode node = new LiteralNode(method, TypeSpecifier.FromType<T>());
            node.InputValuePin.UnconnectedValue = val;
            return node;
        }

        public override string ToString()
        {
            return $"{LiteralType.Name}";
        }
    }
}
