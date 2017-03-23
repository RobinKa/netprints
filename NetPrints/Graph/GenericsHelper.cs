using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    public static class GenericsHelper
    {
        public static TypeSpecifier DetermineTypeNodeType(TypeNode node)
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
        }
    }
}
