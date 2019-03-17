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
        [DataMember]
        public BaseType ElementType
        {
            get;
            private set;
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
        public MakeArrayNode(Method method, BaseType elementType)
            : base(method)
        {
            ElementType = elementType;
            
            AddOutputDataPin(elementType.ShortName, ArrayType);
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
