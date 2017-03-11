using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using NetPrints.Graph;
using NetPrints.Core;

namespace NetPrints.Translator
{
    public class MethodTranslator
    {
        private Dictionary<NodeOutputDataPin, string> variableNames = new Dictionary<NodeOutputDataPin, string>();
        private List<Node> evaluatedNodes = new List<Node>();

        private StringBuilder builder = new StringBuilder();

        private Method method;

        private delegate void NodeTypeHandler(MethodTranslator translator, Node node);

        private Dictionary<Type, NodeTypeHandler> nodeTypeHandlers = new Dictionary<Type, NodeTypeHandler>()
        {
            { typeof(CallMethodNode), (translator, node) => translator.TranslateCallMethodNode(node as CallMethodNode) },
            { typeof(VariableGetterNode), (translator, node) => translator.TranslateVariableGetterNode(node as VariableGetterNode) },
            { typeof(VariableSetterNode), (translator, node) => translator.TranslateVariableSetterNode(node as VariableSetterNode) },
            { typeof(ReturnNode), (translator, node) => translator.TranslateReturnNode(node as ReturnNode) },
            { typeof(MethodEntryNode), (translator, node) => translator.TranslateMethodEntry(node as MethodEntryNode) },
        };

        private string GetOrCreatePinName(NodeOutputDataPin pin)
        {
            if (pin == null)
            {
                return "null";
            }

            if (variableNames.ContainsKey(pin))
            {
                return variableNames[pin];
            }

            string pinName = TranslatorUtil.GetUniqueName(pin.Name, variableNames.Values.ToList());
            variableNames.Add(pin, pinName);
            return pinName;
        }
        
        private IEnumerable<string> GetOrCreatePinNames(IEnumerable<NodeOutputDataPin> pins)
        {
            return pins.Select(pin => GetOrCreatePinName(pin));
        }

        private string GetOrCreateTypedPinName(NodeOutputDataPin pin)
        {
            string pinName = GetOrCreatePinName(pin);
            return $"{pin.PinType.FullName} {pinName}";
        }

        private IEnumerable<string> GetOrCreateTypedPinNames(IEnumerable<NodeOutputDataPin> pins)
        {
            return pins.Select(pin => GetOrCreateTypedPinName(pin));
        }

        public string Translate(Method method)
        {
            this.method = method;

            // Reset state
            variableNames.Clear();
            evaluatedNodes.Clear();
            builder.Clear();

            // Start translation at the method entry node
            TranslateNode(method.MethodEntry);

            return builder.ToString();
        }

        public void TranslateNode(Node node)
        {
            // Make sure all data inputs we depend on have been executed
            foreach (NodeInputDataPin input in node.InputDataPins)
            {
                if (input.IncomingPin != null)
                {
                    if (!evaluatedNodes.Contains(input.IncomingPin.Node))
                    {
                        TranslateNode(input.IncomingPin.Node);
                    }
                }
            }

            evaluatedNodes.Add(node);

            if (nodeTypeHandlers.ContainsKey(node.GetType()))
            {
                nodeTypeHandlers[node.GetType()](this, node);
            }
            else
            {
                Debug.WriteLine($"Unhandled type {node.GetType()} in TranslateNode");
            }
        }

        public void TranslateMethodEntry(MethodEntryNode node)
        {
            // Write the signature of the method

            // Write return type
            if (method.ReturnTypes.Count() > 1)
            {
                builder.Append($"({string.Join(", ", method.ReturnTypes.Select(t => t.FullName))}) ");
            }
            else if(method.ReturnTypes.Count() == 1)
            {
                builder.Append($"{method.ReturnTypes[0].FullName} ");
            }
            else
            {
                builder.Append("void ");
            }

            // Write name
            builder.Append(method.Name);

            // Write parameters
            builder.AppendLine($"({string.Join(",", GetOrCreateTypedPinNames(node.OutputDataPins))})");

            builder.AppendLine("{");

            // Translate the next executed node if any
            if (node.OutputExecPins[0].OutgoingPin != null)
            {
                TranslateNode(node.OutputExecPins[0].OutgoingPin.Node);
            }

            builder.AppendLine("}");
        }
        
        public void TranslateCallMethodNode(CallMethodNode node)
        {
            // Make sure we declare the return values on first execution
            if(evaluatedNodes.Contains(node) && node.OutputDataPins.Count > 0)
            {
                builder.Append("var ");
            }

            // Write assignment of return values
            if (node.OutputDataPins.Count == 1)
            {
                string returnName = GetOrCreatePinName(node.OutputDataPins[0]);

                builder.Append($"{returnName} = ");
            }
            else if (node.OutputDataPins.Count > 1)
            {
                var returnNames = GetOrCreatePinNames(node.OutputDataPins);

                builder.Append($"({string.Join(",", returnNames)}) = ");
            }

            // Write function call with arguments
            var argumentNames = GetOrCreatePinNames(node.InputDataPins.Select(pin => pin.IncomingPin));

            builder.Append($"{node.MethodName}({string.Join(", ", argumentNames)});");

            // Translate the next executed node if any
            if(node.OutputExecPins[0].OutgoingPin != null)
            {
                TranslateNode(node.OutputExecPins[0].OutgoingPin.Node);
            }
        }

        public void TranslateVariableGetterNode(VariableGetterNode node)
        {
            string valueName = GetOrCreatePinName(node.OutputDataPins[0]);
            string targetName = GetOrCreatePinName(node.InputDataPins[0].IncomingPin);

            builder.Append($"var {valueName} = ");

            if(targetName != null)
            {
                builder.Append($"{targetName}.");
            }

            builder.AppendLine($"{node.VariableName};");
        }

        public void TranslateVariableSetterNode(VariableSetterNode node)
        {
            string targetName = GetOrCreatePinName(node.InputDataPins[0].IncomingPin);
            string valueName = GetOrCreatePinName(node.InputDataPins[1].IncomingPin);

            if (targetName != null)
            {
                builder.Append($"{targetName}.");
            }

            builder.AppendLine($"{node.VariableName} = {valueName};");

            variableNames.Add(node.OutputDataPins[0], targetName != null ? $"{targetName}.{node.VariableName}" : node.VariableName);

            // Translate the next executed node if any
            if (node.OutputExecPins[0].OutgoingPin != null)
            {
                TranslateNode(node.OutputExecPins[0].OutgoingPin.Node);
            }
        }

        public void TranslateReturnNode(ReturnNode node)
        {
            if(node.InputDataPins.Count == 0)
            {
                builder.AppendLine("return;");
            }
            else if(node.InputDataPins.Count == 1)
            {
                builder.AppendLine($"return {GetOrCreatePinName(node.InputDataPins[0].IncomingPin)};");
            }
            else
            {
                var returnNames = GetOrCreatePinNames(node.InputDataPins.Select(pin => pin.IncomingPin));
                builder.AppendLine($"return ({string.Join(",", returnNames)});");
            }
        }
    }
}
