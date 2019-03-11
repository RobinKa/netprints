using NetPrints.Core;
using System;
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
        public static bool CanConnectNodePins(NodePin pinA, NodePin pinB, Func<TypeSpecifier, TypeSpecifier, bool> isSubclassOf, bool swapped=false)
        {
            if (pinA is NodeInputExecPin && pinB is NodeOutputExecPin)
            {
                return true;
            }
            else if (pinA is NodeInputDataPin datA && pinB is NodeOutputDataPin datB)
            {
                // Both are TypeSpecifier

                if(datA.PinType is TypeSpecifier typeSpecA && 
                    datB.PinType is TypeSpecifier typeSpecB &&
                    (typeSpecA == typeSpecB || isSubclassOf(typeSpecB, typeSpecA)))
                {
                    return true;
                }
                
                // A is GenericType, B is whatever

                if (datA.PinType is GenericType genTypeA)
                {
                    if(datB.PinType is GenericType genTypeB)
                    {
                        return genTypeA == genTypeB;
                    }
                    else if(datB.PinType is TypeSpecifier typeSpecB2)
                    {
                        return genTypeA == typeSpecB2;
                    }
                }

                // B is GenericType, A is whatever

                if (datB.PinType is GenericType genTypeB2)
                {
                    if (datA.PinType is GenericType genTypeA2)
                    {
                        return genTypeA2 == genTypeB2;
                    }
                    else if (datA.PinType is TypeSpecifier typeSpecA2)
                    {
                        return genTypeB2 == typeSpecA2;
                    }
                }
            }
            else if(!swapped)
            {
                // Try the same for swapped order
                return CanConnectNodePins(pinB, pinA, isSubclassOf, true);
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
    }
}
