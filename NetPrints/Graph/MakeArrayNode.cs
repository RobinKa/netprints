using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Node representing the creation of an array.
    /// </summary>
    [DataContract]
    public class MakeArrayNode : Node
    {
        /// <summary>
        /// Specifier for the type of the elements of the array.
        /// </summary>
        public BaseType ElementType
        {
            get => ElementTypePin.InferredType?.Value ?? TypeSpecifier.FromType<object>();
        }

        /// <summary>
        /// Input type pin for the element type of the array to create.
        /// </summary>
        public NodeInputTypePin ElementTypePin
        {
            get => InputTypePins[0];
        }

        /// <summary>
        /// Output data pin for the created array.
        /// </summary>
        public NodeOutputDataPin ArrayPin
        {
            get => OutputDataPins[0];
        }

        /// <summary>
        /// Specifier for the type of the array.
        /// </summary>
        public TypeSpecifier ArrayType
        {
            get
            {
                if (ElementType is TypeSpecifier typeSpec)
                {
                    return new TypeSpecifier($"{typeSpec.Name}[]", typeSpec.IsEnum, typeSpec.IsInterface, typeSpec.GenericArguments);
                }
                else
                {
                    throw new NotImplementedException("Can only have arrays of TypeSpecifier ElementType.");
                }
                
            }
        }

        /// <summary>
        /// Creates a new node representing the creation of an array.
        /// </summary>
        /// <param name="method">Method the node is part of.</param>
        /// <param name="elementType">Type specifier for the elements of the array.</param>
        public MakeArrayNode(Method method)
            : base(method)
        {
            AddInputTypePin("ElementType");
            AddOutputDataPin("Array", ArrayType);
        }

        protected override void OnInputTypeChanged(object sender, EventArgs eventArgs)
        {
            base.OnInputTypeChanged(sender, eventArgs);

            ArrayPin.PinType.Value = ArrayType;
        }

        /// <summary>
        /// Adds an input data pin for an array element.
        /// </summary>
        public void AddElementPin()
        {
            AddInputDataPin($"Element{InputDataPins.Count}", ElementType);
        }

        /// <summary>
        /// Removes the last input data pin for an array element.
        /// Returns whether one was actually removed.
        /// </summary>
        /// <returns>Whether a pin was removed.</returns>
        public bool RemoveElementPin()
        {
            if (InputDataPins.Count > 0)
            {
                // TODO: Add method for removing pins on Node
                NodeInputDataPin inputDataPin = InputDataPins[InputDataPins.Count - 1];
                GraphUtil.DisconnectInputDataPin(inputDataPin);
                InputDataPins.Remove(inputDataPin);

                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return $"Make {ElementType.Name} Array";
        }
    }
}
