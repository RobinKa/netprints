using NetPrints.Core;
using System.Collections.Generic;
using System.Linq;

namespace NetPrints.Graph
{
    public static class GenericsHelper
    {
        /*public static TypeSpecifier DetermineTypeNodeType(TypeNode node)
        {
            // TODO: Copy node.Type

            // Replace generic arguments with input pins
            foreach (NodeInputTypePin inputTypePin in node.InputTypePins)
            {
                // TODO: Check inputTypePin constraints
                TypeSpecifier inputType = DetermineTypeNodeType(inputTypePin.Node);

                int pinIndex = node.InputTypePins.IndexOf(inputTypePin);
                node.Type.GenericArguments[pinIndex] = inputType;
            }

            return node.Type;
        }*/

        public static BaseType ConstructWithTypePins(BaseType type, IEnumerable<NodeInputTypePin> inputTypePins)
        {
            if (type is TypeSpecifier typeSpecifier)
            {
                // Find types to replace and build dictionary
                Dictionary<GenericType, BaseType> replacementTypes = new Dictionary<GenericType, BaseType>();

                foreach (var inputTypePin in inputTypePins)
                {
                    if (inputTypePin.InferredType?.Value is BaseType replacementType && !(replacementType is null))
                    {
                        GenericType typeToReplace = typeSpecifier.GenericArguments.SingleOrDefault(arg => arg.Name == inputTypePin.Name) as GenericType;

                        // If we can not replace all 
                        if (!(typeToReplace is null))
                        {
                            replacementTypes.Add(typeToReplace, replacementType);
                        }
                    }
                }

                try
                {
                    var constructedType = typeSpecifier.Construct(replacementTypes);
                    return constructedType;
                }
                catch
                {
                    return typeSpecifier;
                }
            }
            else if (type is GenericType genericType)
            {
                BaseType replacementType = inputTypePins.SingleOrDefault(t => t.Name == type.Name)?.InferredType?.Value;
                if (replacementType != null)
                {
                    return replacementType;
                }
            }

            return type;
        }
    }
}
