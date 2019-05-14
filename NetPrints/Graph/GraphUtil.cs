using NetPrints.Base;
using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static bool CanConnectNodePins(INodePin pinA, INodePin pinB, Func<TypeSpecifier, TypeSpecifier, bool> isSubclassOf, Func<TypeSpecifier, TypeSpecifier, bool> hasImplicitCast, bool swapped=false)
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

                if (datA.PinType.Value is TypeSpecifier typeSpecA
                    && datB.PinType.Value is TypeSpecifier typeSpecB
                    && (typeSpecA == typeSpecB || isSubclassOf(typeSpecB, typeSpecA) || hasImplicitCast(typeSpecB, typeSpecA)))
                {
                    return true;
                }

                // A is GenericType, B is whatever

                if (datA.PinType.Value is GenericType genTypeA)
                {
                    if (datB.PinType.Value is GenericType genTypeB)
                    {
                        return genTypeA == genTypeB;
                    }
                    else if (datB.PinType.Value is TypeSpecifier typeSpecB2)
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
            else if (!swapped)
            {
                // Try the same for swapped order
                return CanConnectNodePins(pinB, pinA, isSubclassOf, hasImplicitCast, true);
            }

            return false;
        }

        public static void ConnectPins(INodePin pinA, INodePin pinB)
        {
            // Remove old connections for single connection pins

            if (pinA.ConnectionType == NodePinConnectionType.Single)
            {
                DisconnectPin(pinA);
            }

            if (pinB.ConnectionType == NodePinConnectionType.Single)
            {
                DisconnectPin(pinB);
            }

            // Add new connections
            pinA.ConnectedPins.Add(pinB);
            pinB.ConnectedPins.Add(pinA);
        }

        public static void DisconnectPin(INodePin nodePin)
        {
            foreach (var otherPin in nodePin.ConnectedPins)
            {
                otherPin.ConnectedPins.Remove(nodePin);
            }

            nodePin.ConnectedPins.Clear();
        }

        /// <summary>
        /// Disconnects all pins of a node.
        /// </summary>
        /// <param name="node">Node to have all its pins disconnected.</param>
        public static void DisconnectNodePins(INode node)
        {
            foreach (var pin in node.Pins)
            {
                DisconnectPin(pin);
            }
        }

        public static void DisconnectPins(INodePin a, INodePin b)
        {
            Debug.Assert(a.ConnectedPins.Contains(b) && b.ConnectedPins.Contains(a), "Tried to disconnect two pins which don't connect to each other.");

            a.ConnectedPins.Remove(b);
            b.ConnectedPins.Remove(a);
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

            var rerouteNode = RerouteNode.MakeData(pin.Node.Graph, new Tuple<BaseType, BaseType>[]
            {
                new Tuple<BaseType, BaseType>(pin.PinType, pin.IncomingPin.PinType)
            });

            GraphUtil.ConnectPins(pin.IncomingPin, rerouteNode.InputDataPins[0]);
            GraphUtil.ConnectPins(rerouteNode.OutputDataPins[0], pin);

            return rerouteNode;
        }

        /// <summary>
        /// Adds an execution reroute node and does the necessary rewiring.
        /// </summary>
        /// <param name="pin">Execution pin to add reroute node for.</param>
        /// <returns>Reroute node created for the execution pin.</returns>
        public static RerouteNode AddRerouteNode(NodeOutputExecPin pin)
        {
            if (pin?.OutgoingExecPin == null)
            {
                throw new ArgumentException("Pin or its connected pin were null");
            }

            var rerouteNode = RerouteNode.MakeExecution(pin.Node.Graph, 1);

            GraphUtil.ConnectPins(rerouteNode.OutputExecPins[0], pin.OutgoingExecPin);
            GraphUtil.ConnectPins(pin, rerouteNode.InputExecPins[0]);

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

            var rerouteNode = RerouteNode.MakeType(pin.Node.Graph, 1);

            GraphUtil.ConnectPins(pin.IncomingPin, rerouteNode.InputTypePins[0]);
            GraphUtil.ConnectPins(rerouteNode.OutputTypePins[0], pin);

            return rerouteNode;
        }

        /// <summary>
        /// Creates a type node for the given type. Also recursively
        /// creates any type nodes it takes as generic arguments and
        /// connects them.
        /// </summary>
        /// <param name="graph">Graph to add the type nodes to.</param>
        /// <param name="type">Specifier for the type the type node should output.</param>
        /// <param name="x">X position of the created type node.</param>
        /// <param name="y">Y position of the created type node.</param>
        /// <returns>Type node outputting the given type.</returns>
        public static TypeNode CreateNestedTypeNode(NodeGraph graph, BaseType type, double x, double y)
        {
            const double offsetX = -308;
            const double offsetY = -112;

            var typeNode = new TypeNode(graph, type)
            {
                PositionX = x,
                PositionY = y,
            };

            // Create nodes for the type's generic arguments and connect
            // them to it.
            if (type is TypeSpecifier typeSpecifier)
            {
                IEnumerable<TypeNode> genericArgNodes = typeSpecifier.GenericArguments.Select(arg => CreateNestedTypeNode(graph, arg, x + offsetX, y + offsetY * (typeSpecifier.GenericArguments.IndexOf(arg) + 1)));

                foreach (TypeNode genericArgNode in genericArgNodes)
                {
                    GraphUtil.ConnectPins(genericArgNode.OutputTypePins[0], typeNode.InputTypePins[0]);
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
        public static MethodGraph AddOverrideMethod(ClassGraph cls, MethodSpecifier methodSpecifier)
        {
            if (cls.Methods.Any(m => m.Name == methodSpecifier.Name)
                || !(methodSpecifier.Modifiers.HasFlag(MethodModifiers.Virtual)
                || methodSpecifier.Modifiers.HasFlag(MethodModifiers.Override)
                || methodSpecifier.Modifiers.HasFlag(MethodModifiers.Abstract)))
            {
                return null;
            }

            // Remove virtual & abstract flag and add override flag
            MethodModifiers modifiers = methodSpecifier.Modifiers;
            modifiers &= ~(MethodModifiers.Virtual | MethodModifiers.Abstract);
            modifiers |= MethodModifiers.Override;

            // Create method
            MethodGraph newMethod = new MethodGraph(methodSpecifier.Name)
            {
                Class = cls,
                Modifiers = modifiers,
                Visibility = methodSpecifier.Visibility
            };

            // Set position of entry and return node
            newMethod.EntryNode.PositionX = 560;
            newMethod.EntryNode.PositionY = 504;
            newMethod.ReturnNodes.First().PositionX = newMethod.EntryNode.PositionX + 672;
            newMethod.ReturnNodes.First().PositionY = newMethod.EntryNode.PositionY;

            // Connect entry and return node execution pins
            GraphUtil.ConnectPins(newMethod.EntryNode.InitialExecutionPin, newMethod.MainReturnNode.ReturnPin);

            // Add generic arguments
            for (var i = 0; i < methodSpecifier.GenericArguments.Count; i++)
            {
                newMethod.MethodEntryNode.AddGenericArgument();
            }

            const int offsetX = -308;
            const int offsetY = -112;

            // Add argument pins, their type nodes and connect them
            for (int i = 0; i < methodSpecifier.Parameters.Count; i++)
            {
                BaseType argType = methodSpecifier.Parameters[i].Value;
                TypeNode argTypeNode = CreateNestedTypeNode(newMethod, argType, newMethod.EntryNode.PositionX + offsetX, newMethod.EntryNode.PositionY + offsetY * (i+1));

                newMethod.MethodEntryNode.AddArgument();

                ConnectPins(argTypeNode.OutputTypePins[0], newMethod.EntryNode.InputTypePins[i]);
            }

            // Add return types, their type nodes and connect them
            for (int i = 0; i < methodSpecifier.ReturnTypes.Count; i++)
            {
                BaseType returnType = methodSpecifier.ReturnTypes[i];
                TypeNode returnTypeNode = CreateNestedTypeNode(newMethod, returnType, newMethod.MainReturnNode.PositionX + offsetX, newMethod.MainReturnNode.PositionY + offsetY * (i+1));

                newMethod.MainReturnNode.AddReturnType();

                ConnectPins(returnTypeNode.OutputTypePins[0], newMethod.MainReturnNode.InputTypePins[i]);
            }

            cls.Methods.Add(newMethod);

            return newMethod;
        }

        /// <summary>
        /// Connects a pin to the first possible pin of the passed node.
        /// </summary>
        /// <param name="pin">Pin to connect</param>
        /// <param name="node">Node to connect the pin to.</param>
        public static void ConnectRelevantPins(NodePin pin, Node node, Func<TypeSpecifier, TypeSpecifier, bool> isSubclassOf,
            Func<TypeSpecifier, TypeSpecifier, bool> hasImplicitCast)
        {
            if (pin is NodeInputExecPin ixp)
            {
                GraphUtil.ConnectPins(node.OutputExecPins[0], ixp);
            }
            else if (pin is NodeOutputExecPin oxp)
            {
                GraphUtil.ConnectPins(oxp, node.InputExecPins[0]);
            }
            else if (pin is NodeInputDataPin idp)
            {
                foreach (var otherOtp in node.OutputDataPins)
                {
                    if (GraphUtil.CanConnectNodePins(otherOtp, idp,isSubclassOf, hasImplicitCast))
                    {
                        GraphUtil.ConnectPins(otherOtp, idp);

                        // Connect exec pins if possible.
                        // Also forward the previous connection through the new node.
                        if (pin.Node.InputExecPins.Count > 0 && node.OutputExecPins.Count > 0)
                        {
                            var oldConnected = pin.Node.InputExecPins[0].IncomingExecutionPins.FirstOrDefault();

                            if (oldConnected != null)
                            {
                                GraphUtil.DisconnectPin(oldConnected);
                            }

                            GraphUtil.ConnectPins(node.OutputExecPins[0], pin.Node.InputExecPins[0]);

                            if (oldConnected != null && node.InputExecPins.Count > 0)
                            {
                                GraphUtil.ConnectPins(oldConnected, node.InputExecPins[0]);
                            }
                        }

                        break;
                    }
                }
            }
            else if (pin is NodeOutputDataPin odp)
            {
                foreach (var otherIdp in node.InputDataPins)
                {
                    if (GraphUtil.CanConnectNodePins(odp, otherIdp, isSubclassOf, hasImplicitCast))
                    {
                        GraphUtil.ConnectPins(odp, otherIdp);

                        // Connect exec pins if possible.
                        // Also forward the previous connection through the new node.
                        if (node.InputExecPins.Count > 0 && pin.Node.OutputExecPins.Count > 0)
                        {
                            var oldConnected = pin.Node.OutputExecPins[0].OutgoingExecPin;

                            GraphUtil.ConnectPins(pin.Node.OutputExecPins[0], node.InputExecPins[0]);

                            if (oldConnected != null && node.OutputExecPins.Count > 0)
                            {
                                GraphUtil.ConnectPins(node.OutputExecPins[0], oldConnected);
                            }
                        }

                        break;
                    }
                }
            }
            else if (pin is NodeInputTypePin itp)
            {
                if (node.OutputTypePins.Count > 0)
                {
                    GraphUtil.ConnectPins(node.OutputTypePins[0], itp);
                }
            }
            else if (pin is NodeOutputTypePin otp)
            {
                if (node.InputTypePins.Count > 0)
                {
                    GraphUtil.ConnectPins(otp, node.InputTypePins[0]);
                }
            }
        }
    }
}
