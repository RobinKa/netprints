using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NetPrints.Graph
{
    public static class GraphUtil
    {
        /// <summary>
        /// Splits camel-case names into words seperated by spaces.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SplitCamelCase(string input)
        {
            return Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }

        /// <summary>
        /// Determines whether two node pins can be connected to each other.
        /// </summary>
        /// <param name="pinA">First pin.</param>
        /// <param name="pinB">Second pin.</param>
        /// <param name="isSubclassOf">Function for determining whether one type is the subclass of another type.</param>
        /// <param name="swapped">Whether we want pinB to be the first pin and vice versa.</param>
        /// <returns></returns>
        public static bool CanConnectNodePins(NodePin pinA, NodePin pinB, Func<TypeSpecifier, TypeSpecifier, bool> isSubclassOf, Func<TypeSpecifier, TypeSpecifier, bool> hasImplicitCast, bool swapped=false)
        {
            if (pinA is NodeInputExecPin && pinB is NodeOutputExecPin)
            {
                return true;
            }
            else if (pinA is NodeInputTypePin && pinB is NodeOutputTypePin)
            {
                // TODO: Check for constraints
                return true;
            }
            else if (pinA is NodeInputDataPin datA && pinB is NodeOutputDataPin datB)
            {
                // Both are TypeSpecifier

                if(datA.PinType.Value is TypeSpecifier typeSpecA && 
                    datB.PinType.Value is TypeSpecifier typeSpecB &&
                    (typeSpecA == typeSpecB || isSubclassOf(typeSpecB, typeSpecA) || hasImplicitCast(typeSpecB, typeSpecA)))
                {
                    return true;
                }
                
                // A is GenericType, B is whatever

                if (datA.PinType.Value is GenericType genTypeA)
                {
                    if(datB.PinType.Value is GenericType genTypeB)
                    {
                        return genTypeA == genTypeB;
                    }
                    else if(datB.PinType.Value is TypeSpecifier typeSpecB2)
                    {
                        return genTypeA == typeSpecB2;
                    }
                }

                // B is GenericType, A is whatever

                if (datB.PinType.Value is GenericType genTypeB2)
                {
                    if (datA.PinType.Value is GenericType genTypeA2)
                    {
                        return genTypeA2 == genTypeB2;
                    }
                    else if (datA.PinType.Value is TypeSpecifier typeSpecA2)
                    {
                        return genTypeB2 == typeSpecA2;
                    }
                }
            }
            else if(!swapped)
            {
                // Try the same for swapped order
                return CanConnectNodePins(pinB, pinA, isSubclassOf, hasImplicitCast, true);
            }

            return false;
        }

        /// <summary>
        /// Connects two node pins together. Makes sure any previous connections will be disconnected.
        /// If the pin types are not compatible an ArgumentException will be thrown.
        /// </summary>
        /// <param name="pinA">First pin.</param>
        /// <param name="pinB">Second pin.</param>
        /// <param name="swapped">Whether we want pinB to be the first pin and vice versa.</param>
        public static void ConnectNodePins(NodePin pinA, NodePin pinB, bool swapped=false)
        {
            if (pinA is NodeInputExecPin exA && pinB is NodeOutputExecPin exB)
            {
                ConnectExecPins(exB, exA);
            }
            else if (pinA is NodeInputDataPin datA && pinB is NodeOutputDataPin datB)
            {
                ConnectDataPins(datB, datA);
            }
            else if (pinA is NodeInputTypePin typA && pinB is NodeOutputTypePin typB)
            {
                ConnectTypePins(typB, typA);
            }
            else if (!swapped)
            {
                ConnectNodePins(pinB, pinA, true);
            }
            else
            {
                throw new ArgumentException("The passed pins can not be connected because their types are incompatible.");
            }
        }

        /// <summary>
        /// Connects two node execution pins. Removes any previous connection.
        /// </summary>
        /// <param name="fromPin">Output execution pin to connect.</param>
        /// <param name="toPin">Input execution pin to connect.</param>
        public static void ConnectExecPins(NodeOutputExecPin fromPin, NodeInputExecPin toPin)
        {
            // Remove from old pin if any
            if(fromPin.OutgoingPin != null)
            {
                fromPin.OutgoingPin.IncomingPins.Remove(fromPin);
            }

            fromPin.OutgoingPin = toPin;
            toPin.IncomingPins.Add(fromPin);
        }

        /// <summary>
        /// Connects two node data pins. Removes any previous connection.
        /// </summary>
        /// <param name="fromPin">Output data pin to connect.</param>
        /// <param name="toPin">Input data pin to connect.</param>
        public static void ConnectDataPins(NodeOutputDataPin fromPin, NodeInputDataPin toPin)
        {
            // Remove from old pin if any
            if(toPin.IncomingPin != null)
            {
                toPin.IncomingPin.OutgoingPins.Remove(toPin);
            }

            fromPin.OutgoingPins.Add(toPin);
            toPin.IncomingPin = fromPin;
        }

        /// <summary>
        /// Connects two node type pins. Removes any previous connection.
        /// </summary>
        /// <param name="fromPin">Output type pin to connect.</param>
        /// <param name="toPin">Input type pin to connect.</param>
        public static void ConnectTypePins(NodeOutputTypePin fromPin, NodeInputTypePin toPin)
        {
            // Remove from old pin if any
            if (toPin.IncomingPin != null)
            {
                toPin.IncomingPin.OutgoingPins.Remove(toPin);
            }

            fromPin.OutgoingPins.Add(toPin);
            toPin.IncomingPin = fromPin;
        }

        /// <summary>
        /// Disconnects all pins of a node.
        /// </summary>
        /// <param name="node">Node to have all its pins disconnected.</param>
        public static void DisconnectNodePins(Node node)
        {
            foreach (NodeInputDataPin pin in node.InputDataPins)
            {
                DisconnectInputDataPin(pin);
            }

            foreach (NodeOutputDataPin pin in node.OutputDataPins)
            {
                DisconnectOutputDataPin(pin);
            }

            foreach (NodeInputExecPin pin in node.InputExecPins)
            {
                DisconnectInputExecPin(pin);
            }

            foreach (NodeOutputExecPin pin in node.OutputExecPins)
            {
                DisconnectOutputExecPin(pin);
            }

            foreach (NodeInputTypePin pin in node.InputTypePins)
            {
                DisconnectInputTypePin(pin);
            }

            foreach (NodeOutputTypePin pin in node.OutputTypePins)
            {
                DisconnectOutputTypePin(pin);
            }
        }

        public static void DisconnectPin(NodePin nodePin)
        {
            if (nodePin is NodeInputDataPin idp)
            {
                DisconnectInputDataPin(idp);
            }
            else if (nodePin is NodeOutputDataPin odp)
            {
                DisconnectOutputDataPin(odp);
            }
            else if (nodePin is NodeInputExecPin ixp)
            {
                DisconnectInputExecPin(ixp);
            }
            else if (nodePin is NodeOutputExecPin oxp)
            {
                DisconnectOutputExecPin(oxp);
            }
            else if (nodePin is NodeInputTypePin itp)
            {
                DisconnectInputTypePin(itp);
            }
            else if (nodePin is NodeOutputTypePin otp)
            {
                DisconnectOutputTypePin(otp);
            }
            else
            {
                throw new NotImplementedException("Unknown pin type to disconnect.");
            }
        }

        public static void DisconnectInputDataPin(NodeInputDataPin pin)
        {
            pin.IncomingPin?.OutgoingPins.Remove(pin);
            pin.IncomingPin = null;
        }

        public static void DisconnectOutputDataPin(NodeOutputDataPin pin)
        {
            foreach(NodeInputDataPin outgoingPin in pin.OutgoingPins)
            {
                outgoingPin.IncomingPin = null;
            }

            pin.OutgoingPins.Clear();
        }

        public static void DisconnectInputTypePin(NodeInputTypePin pin)
        {
            pin.IncomingPin?.OutgoingPins.Remove(pin);
            pin.IncomingPin = null;
        }

        public static void DisconnectOutputTypePin(NodeOutputTypePin pin)
        {
            foreach (NodeInputTypePin outgoingPin in pin.OutgoingPins)
            {
                outgoingPin.IncomingPin = null;
            }

            pin.OutgoingPins.Clear();
        }

        public static void DisconnectOutputExecPin(NodeOutputExecPin pin)
        {
            pin.OutgoingPin?.IncomingPins.Remove(pin);
            pin.OutgoingPin = null;
        }

        public static void DisconnectInputExecPin(NodeInputExecPin pin)
        {
            foreach (NodeOutputExecPin incomingPin in pin.IncomingPins)
            {
                incomingPin.OutgoingPin = null;
            }

            pin.IncomingPins.Clear();
        }


        /// <summary>
        /// Adds a data reroute node and does the necessary rewiring.
        /// </summary>
        /// <param name="pin">Data pin to add reroute node for.</param>
        /// <returns>Reroute node created for the data pin.</returns>
        public static RerouteNode AddRerouteNode(NodeInputDataPin pin)
        {
            if (pin?.IncomingPin == null)
            {
                throw new ArgumentException("Pin or its connected pin were null");
            }

            var rerouteNode = RerouteNode.MakeData(pin.Node.Method, new Tuple<BaseType, BaseType>[]
            {
                new Tuple<BaseType, BaseType>(pin.PinType, pin.IncomingPin.PinType)
            });

            GraphUtil.ConnectDataPins(pin.IncomingPin, rerouteNode.InputDataPins[0]);
            GraphUtil.ConnectDataPins(rerouteNode.OutputDataPins[0], pin);

            return rerouteNode;
        }

        /// <summary>
        /// Adds an execution reroute node and does the necessary rewiring.
        /// </summary>
        /// <param name="pin">Execution pin to add reroute node for.</param>
        /// <returns>Reroute node created for the execution pin.</returns>
        public static RerouteNode AddRerouteNode(NodeOutputExecPin pin)
        {
            if (pin?.OutgoingPin == null)
            {
                throw new ArgumentException("Pin or its connected pin were null");
            }

            var rerouteNode = RerouteNode.MakeExecution(pin.Node.Method, 1);

            GraphUtil.ConnectExecPins(rerouteNode.OutputExecPins[0], pin.OutgoingPin);
            GraphUtil.ConnectExecPins(pin, rerouteNode.InputExecPins[0]);

            return rerouteNode;
        }

        /// <summary>
        /// Adds a type reroute node and does the necessary rewiring.
        /// </summary>
        /// <param name="pin">Type pin to add reroute node for.</param>
        /// <returns>Reroute node created for the type pin.</returns>
        public static RerouteNode AddRerouteNode(NodeInputTypePin pin)
        {
            if (pin?.IncomingPin == null)
            {
                throw new ArgumentException("Pin or its connected pin were null");
            }

            var rerouteNode = RerouteNode.MakeType(pin.Node.Method, 1);

            GraphUtil.ConnectTypePins(pin.IncomingPin, rerouteNode.InputTypePins[0]);
            GraphUtil.ConnectTypePins(rerouteNode.OutputTypePins[0], pin);

            return rerouteNode;
        }

        /// <summary>
        /// Creates a type node for the given type. Also recursively
        /// creates any type nodes it takes as generic arguments and
        /// connects them.
        /// </summary>
        /// <param name="method">Method to add the type nodes to.</param>
        /// <param name="type">Specifier for the type the type node should output.</param>
        /// <param name="x">X position of the created type node.</param>
        /// <param name="y">Y position of the created type node.</param>
        /// <returns>Type node outputting the given type.</returns>
        public static TypeNode CreateNestedTypeNode(Method method, BaseType type, double x, double y)
        {
            double offsetX = -300;
            double offsetY = -100;

            var typeNode = new TypeNode(method, type)
            {
                PositionX = x,
                PositionY = y,
            };

            // Create nodes for the type's generic arguments and connect
            // them to it.
            if (type is TypeSpecifier typeSpecifier)
            {
                IEnumerable<TypeNode> genericArgNodes = typeSpecifier.GenericArguments.Select(arg => CreateNestedTypeNode(method, arg, x + offsetX, y + offsetY * (typeSpecifier.GenericArguments.IndexOf(arg) + 1)));

                foreach (TypeNode genericArgNode in genericArgNodes)
                {
                    GraphUtil.ConnectTypePins(genericArgNode.OutputTypePins[0], typeNode.InputTypePins[0]);
                }
            }

            return typeNode;
        }

        /// <summary>
        /// Adds an override method for the given method specifier to
        /// the given class.
        /// </summary>
        /// <param name="cls">Class to add the method to.</param>
        /// <param name="methodSpecifier">Method specifier for the method to override.</param>
        /// <returns>Method in the class that represents the overriding method.</returns>
        public static Method AddOverrideMethod(Class cls, MethodSpecifier methodSpecifier)
        {
            if (cls.Methods.Any(m => m.Name == methodSpecifier.Name) ||
                !(methodSpecifier.Modifiers.HasFlag(MethodModifiers.Virtual) ||
                methodSpecifier.Modifiers.HasFlag(MethodModifiers.Override) ||
                methodSpecifier.Modifiers.HasFlag(MethodModifiers.Abstract)))
            {
                return null;
            }

            // Remove virtual & abstract flag and add override flag
            MethodModifiers modifiers = methodSpecifier.Modifiers;
            modifiers &= ~(MethodModifiers.Virtual | MethodModifiers.Abstract);
            modifiers |= MethodModifiers.Override;

            // Create method
            Method newMethod = new Method(methodSpecifier.Name)
            {
                Class = cls,
                Modifiers = modifiers
            };

            // Set position of entry and return node
            newMethod.EntryNode.PositionX = 500;
            newMethod.EntryNode.PositionY = 500;
            newMethod.ReturnNodes.First().PositionX = newMethod.EntryNode.PositionX + 1000;
            newMethod.ReturnNodes.First().PositionY = newMethod.EntryNode.PositionY;

            // Connect entry and return node execution pins
            GraphUtil.ConnectExecPins(newMethod.EntryNode.InitialExecutionPin, newMethod.MainReturnNode.ReturnPin);

            // Add generic arguments
            for (var i = 0; i < methodSpecifier.GenericArguments.Count; i++)
            {
                newMethod.EntryNode.AddGenericArgument();
            }

            int offsetX = -300;
            int offsetY = -100;

            // Add argument pins, their type nodes and connect them
            for (int i = 0; i < methodSpecifier.Arguments.Count; i++)
            {
                BaseType argType = methodSpecifier.Arguments[i].Value;
                TypeNode argTypeNode = CreateNestedTypeNode(newMethod, argType, newMethod.EntryNode.PositionX + offsetX, newMethod.EntryNode.PositionY + offsetY * (i+1));

                newMethod.EntryNode.AddArgument();

                ConnectTypePins(argTypeNode.OutputTypePins[0], newMethod.EntryNode.InputTypePins[i]);
            }

            // Add return types, their type nodes and connect them
            for (int i = 0; i < methodSpecifier.ReturnTypes.Count; i++)
            {
                BaseType returnType = methodSpecifier.ReturnTypes[i];
                TypeNode returnTypeNode = CreateNestedTypeNode(newMethod, returnType, newMethod.MainReturnNode.PositionX + offsetX, newMethod.MainReturnNode.PositionY + offsetY * (i+1));

                newMethod.MainReturnNode.AddReturnType();

                ConnectTypePins(returnTypeNode.OutputTypePins[0], newMethod.MainReturnNode.InputTypePins[i]);
            }

            cls.Methods.Add(newMethod);

            return newMethod;
        }
    }
}
