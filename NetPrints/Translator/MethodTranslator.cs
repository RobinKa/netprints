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
        private Dictionary<Node, List<int>> nodeStateIds = new Dictionary<Node, List<int>>();
        private int nextStateId = 0;
        private IEnumerable<Node> execNodes = new List<Node>();
        private IEnumerable<Node> nodes = new List<Node>();
        private HashSet<NodeInputExecPin> pinsJumpedTo = new HashSet<NodeInputExecPin>();

        private int jumpTableStateId;
        
        private StringBuilder builder = new StringBuilder();

        private Method method;

        private delegate void NodeTypeHandler(MethodTranslator translator, Node node);

        private Dictionary<Type, List<NodeTypeHandler>> nodeTypeHandlers = new Dictionary<Type, List<NodeTypeHandler>>()
        {
            { typeof(CallMethodNode), new List<NodeTypeHandler> { (translator, node) => translator.TranslateCallMethodNode(node as CallMethodNode) } },
            { typeof(VariableSetterNode), new List<NodeTypeHandler> { (translator, node) => translator.TranslateVariableSetterNode(node as VariableSetterNode) } },
            { typeof(ReturnNode), new List<NodeTypeHandler> { (translator, node) => translator.TranslateReturnNode(node as ReturnNode) } },
            { typeof(EntryNode), new List<NodeTypeHandler> { (translator, node) => translator.TranslateMethodEntry(node as EntryNode) } },
            { typeof(IfElseNode), new List<NodeTypeHandler> { (translator, node) => translator.TranslateIfElseNode(node as IfElseNode) } },
            { typeof(CallStaticFunctionNode), new List<NodeTypeHandler> { (translator, node) => translator.TranslateCallStaticFunctionNode(node as CallStaticFunctionNode) } },

            { typeof(ForLoopNode), new List<NodeTypeHandler> {
                (translator, node) => translator.TranslateStartForLoopNode(node as ForLoopNode),
                (translator, node) => translator.TranslateNextForLoopNode(node as ForLoopNode)} },

            { typeof(VariableGetterNode), new List<NodeTypeHandler> { (translator, node) => translator.PureTranslateVariableGetterNode(node as VariableGetterNode) } },
            { typeof(LiteralNode), new List<NodeTypeHandler> { (translator, node) => translator.PureTranslateLiteralNode(node as LiteralNode) } },
        };

        private int GetNextStateId()
        {
            return nextStateId++;
        }

        private int GetExecPinStateId(NodeInputExecPin pin)
        {
            return nodeStateIds[pin.Node][pin.Node.InputExecPins.IndexOf(pin)];
        }

        private string GetOrCreatePinName(NodeOutputDataPin pin)
        {
            // Return the default value of the pin type if nothing is connected
            if (pin == null)
            {
                return "null";
            }

            if (variableNames.ContainsKey(pin))
            {
                return variableNames[pin];
            }

            string pinName = TranslatorUtil.GetUniqueVariableName(pin.Name, variableNames.Values.ToList());
            variableNames.Add(pin, pinName);
            return pinName;
        }

        private string GetPinIncomingValue(NodeInputDataPin pin)
        {
            if(pin.IncomingPin == null)
            {
                return $"default({pin.PinType.FullName})";
            }
            else
            {
                return GetOrCreatePinName(pin.IncomingPin);
            }
        }
        
        private IEnumerable<string> GetOrCreatePinNames(IEnumerable<NodeOutputDataPin> pins)
        {
            return pins.Select(pin => GetOrCreatePinName(pin)).ToList();
        }

        private string GetOrCreateTypedPinName(NodeOutputDataPin pin)
        {
            string pinName = GetOrCreatePinName(pin);
            return $"{pin.PinType.FullName} {pinName}";
        }

        private IEnumerable<string> GetOrCreateTypedPinNames(IEnumerable<NodeOutputDataPin> pins)
        {
            return pins.Select(pin => GetOrCreateTypedPinName(pin)).ToList();
        }

        private void CreateStates()
        {
            foreach(Node node in execNodes)
            {
                if (!(node is EntryNode))
                {
                    nodeStateIds.Add(node, new List<int>());

                    foreach (NodeInputExecPin execPin in node.InputExecPins)
                    {
                        nodeStateIds[node].Add(GetNextStateId());
                    }
                }
            }
        }

        private void CreateVariables()
        {
            foreach(Node node in nodes)
            {
                var v = GetOrCreatePinNames(node.OutputDataPins);
            }
        }

        private void TranslateVariables()
        {
            foreach (var v in variableNames)
            {
                NodeOutputDataPin pin = v.Key;
                string variableName = v.Value;

                if (!(pin.Node is EntryNode))
                {
                    builder.AppendLine($"{pin.PinType.FullName} {variableName};");
                }
            }
        }

        private void TranslateSignature()
        {
            // Write modifiers
            if (method.Modifiers.HasFlag(MethodModifiers.Protected))
            {
                builder.Append("protected ");
            }
            else
            {
                if (method.Modifiers.HasFlag(MethodModifiers.Public))
                {
                    builder.Append("public ");
                }
            }

            if(method.Modifiers.HasFlag(MethodModifiers.Static))
            {
                builder.Append("static ");
            }

            if(method.Modifiers.HasFlag(MethodModifiers.Abstract))
            {
                builder.Append("abstract ");
            }

            if(method.Modifiers.HasFlag(MethodModifiers.Sealed))
            {
                builder.Append("sealed ");
            }

            // Write return type
            if (method.ReturnTypes.Count() > 1)
            {
                // Tuple<Types..> (won't be needed in the future)
                string returnType = typeof(Tuple).FullName + "<" + string.Join(", ", method.ReturnTypes.Select(t => t.FullName)) + ">";
                builder.Append(returnType + " ");

                //builder.Append($"({string.Join(", ", method.ReturnTypes.Select(t => t.FullName))}) ");
            }
            else if (method.ReturnTypes.Count() == 1)
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
            builder.AppendLine($"({string.Join(", ", GetOrCreateTypedPinNames(method.EntryNode.OutputDataPins))})");
        }

        private void TranslateJumpTable()
        {
            builder.AppendLine($"State{jumpTableStateId}:");
            builder.AppendLine("if(jumpStack.Count == 0) throw new System.Exception();");
            builder.AppendLine("switch(jumpStack.Pop())");
            builder.AppendLine("{");

            foreach (NodeInputExecPin pin in pinsJumpedTo)
            {
                builder.AppendLine($"case {GetExecPinStateId(pin)}:");
                WriteGotoInputPin(pin);
            }

            builder.AppendLine("default:");
            builder.AppendLine("throw new System.Exception();");

            builder.AppendLine("}"); // End switch
        }

        public string Translate(Method method)
        {
            this.method = method;

            // Reset state
            variableNames.Clear();
            nodeStateIds.Clear();
            nextStateId = 0;
            builder.Clear();

            nodes = TranslatorUtil.GetAllNodesInMethod(method);
            execNodes = TranslatorUtil.GetExecNodesInMethod(method);

            // Assign a state id to every non-pure node
            CreateStates();

            // Assign jump table state id
            // Write it later once we know which states get jumped to
            jumpTableStateId = GetNextStateId();
            
            // Create variables for all output pins for every node
            CreateVariables();

            // Write the signatures
            TranslateSignature();
            builder.AppendLine("{"); // Method start

            builder.AppendLine("System.Collections.Generic.Stack<int> jumpStack = new System.Collections.Generic.Stack<int>();");

            // Write the variable declarations
            TranslateVariables();
            builder.AppendLine();

            // Start at node after method entry
            WriteGotoOutputPin(method.EntryNode.OutputExecPins[0]);
            builder.AppendLine();
            
            // Translate every exec node
            foreach (Node node in execNodes)
            {
                if (!(node is EntryNode))
                {
                    for (int pinIndex = 0; pinIndex < node.InputExecPins.Count; pinIndex++)
                    {
                        builder.AppendLine($"State{(nodeStateIds[node][pinIndex])}:");
                        TranslateNode(node, pinIndex);
                        builder.AppendLine();
                    }
                }
            }

            // Write the jump table
            TranslateJumpTable();
            
            builder.AppendLine("}"); // Method end

            return builder.ToString();
        }

        public void TranslateNode(Node node, int pinIndex)
        {
            if (nodeTypeHandlers.ContainsKey(node.GetType()))
            {
                nodeTypeHandlers[node.GetType()][pinIndex](this, node);
            }
            else
            {
                Debug.WriteLine($"Unhandled type {node.GetType()} in TranslateNode");
            }
        }

        private void WriteGotoJumpTable()
        {
            builder.AppendLine($"goto State{jumpTableStateId};");
        }

        private void WritePushJumpStack(NodeInputExecPin pin)
        {
            if (!pinsJumpedTo.Contains(pin))
            {
                pinsJumpedTo.Add(pin);
            }

            builder.AppendLine($"jumpStack.Push({GetExecPinStateId(pin)});");
        }

        private void WriteGotoInputPin(NodeInputExecPin pin)
        {
            builder.AppendLine($"goto State{GetExecPinStateId(pin)};");
        }

        private void WriteGotoOutputPin(NodeOutputExecPin pin)
        {
            if(pin.OutgoingPin == null)
            {
                WriteGotoJumpTable();
            }
            else
            {
                WriteGotoInputPin(pin.OutgoingPin);
            }
        }

        public void TranslateDependentPureNodes(Node node)
        {
            var sortedPureNodes = TranslatorUtil.GetSortedPureNodes(node);
            foreach(Node depNode in sortedPureNodes)
            {
                TranslateNode(depNode, 0);
            }
        }

        public void TranslateMethodEntry(EntryNode node)
        {
            // Go to the next state
            WriteGotoOutputPin(node.OutputExecPins[0]);
        }
        
        public void TranslateCallMethodNode(CallMethodNode node)
        {
            // Translate all the pure nodes this node depends on in
            // the correct order
            TranslateDependentPureNodes(node);

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

            // Write target
            if (node.TargetPin.IncomingPin != null)
            {
                string targetName = GetOrCreatePinName(node.TargetPin.IncomingPin);
                builder.Append($"{targetName}.");
            }
            else
            {
                // Default to this
                builder.Append("this.");
            }

            // Write function call with arguments
            var argumentNames = GetOrCreatePinNames(node.ArgumentPins.Select(pin => pin.IncomingPin));

            builder.AppendLine($"{node.MethodName}({string.Join(", ", argumentNames)});");

            // Go to the next state
            WriteGotoOutputPin(node.OutputExecPins[0]);
        }

        public void TranslateCallStaticFunctionNode(CallStaticFunctionNode node)
        {
            // Translate all the pure nodes this node depends on in
            // the correct order
            TranslateDependentPureNodes(node);

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

            // Write class name / target
            if (node.ClassName != null)
            {
                builder.Append($"{node.ClassName}.");
            }

            // Write function call with arguments
            var argumentNames = GetOrCreatePinNames(node.ArgumentPins.Select(pin => pin.IncomingPin));

            builder.AppendLine($"{node.MethodName}({string.Join(", ", argumentNames)});");

            // Go to the next state
            WriteGotoOutputPin(node.OutputExecPins[0]);
        }

        public void TranslateVariableSetterNode(VariableSetterNode node)
        {
            // Translate all the pure nodes this node depends on in
            // the correct order
            TranslateDependentPureNodes(node);
            
            string valueName = GetOrCreatePinName(node.InputDataPins[1].IncomingPin);

            if (node.InputDataPins[0].IncomingPin != null)
            {
                string targetName = GetOrCreatePinName(node.InputDataPins[0].IncomingPin);
                builder.Append($"{targetName}.");
            }
            else
            {
                builder.Append("this.");
            }

            builder.AppendLine($"{node.VariableName} = {valueName};");

            // Set output pin of this node to the same value
            builder.AppendLine($"{GetOrCreatePinName(node.OutputDataPins[0])} = {valueName};");

            // Go to the next state
            WriteGotoOutputPin(node.OutputExecPins[0]);
        }

        public void TranslateReturnNode(ReturnNode node)
        {
            // Translate all the pure nodes this node depends on in
            // the correct order
            TranslateDependentPureNodes(node);

            if (node.InputDataPins.Count == 0)
            {
                builder.AppendLine("return;");
            }
            else if(node.InputDataPins.Count == 1)
            {
                builder.AppendLine($"return {GetPinIncomingValue(node.InputDataPins[0])};");
            }
            else
            {
                var returnValues = node.InputDataPins.Select(pin => GetPinIncomingValue(pin));

                // Tuple<Types..> (won't be needed in the future)
                string returnType = typeof(Tuple).FullName + "<" + string.Join(", ", node.InputDataPins.Select(pin => pin.PinType.FullName)) + ">";
                builder.AppendLine($"return new {returnType}({string.Join(",", returnValues)});");
            }
        }

        public void TranslateIfElseNode(IfElseNode node)
        {
            // Translate all the pure nodes this node depends on in
            // the correct order
            TranslateDependentPureNodes(node);

            string conditionVar = node.ConditionPin.IncomingPin != null ? 
                GetOrCreatePinName(node.ConditionPin.IncomingPin) : "false";
            
            builder.AppendLine($"if({conditionVar})");
            builder.AppendLine("{");
            WriteGotoOutputPin(node.TruePin);
            builder.AppendLine("}");
            builder.AppendLine("else");
            builder.AppendLine("{");
            WriteGotoOutputPin(node.FalsePin);
            builder.AppendLine("}");
        }

        public void TranslateStartForLoopNode(ForLoopNode node)
        {
            // Translate all the pure nodes this node depends on in
            // the correct order
            TranslateDependentPureNodes(node);
            
            builder.AppendLine($"{GetOrCreatePinName(node.IndexPin)} = {GetPinIncomingValue(node.InitialIndexPin)};");
            builder.AppendLine($"if({GetOrCreatePinName(node.IndexPin)} < {GetPinIncomingValue(node.MaxIndexPin)})");
            builder.AppendLine("{");
            WritePushJumpStack(node.NextLoopPin);
            WriteGotoOutputPin(node.LoopPin);
            builder.AppendLine("}");
        }

        public void TranslateNextForLoopNode(ForLoopNode node)
        {
            // Translate all the pure nodes this node depends on in
            // the correct order
            TranslateDependentPureNodes(node);

            builder.AppendLine($"{GetOrCreatePinName(node.IndexPin)}++;");
            builder.AppendLine($"if({GetOrCreatePinName(node.IndexPin)} < {GetPinIncomingValue(node.MaxIndexPin)})");
            builder.AppendLine("{");
            WritePushJumpStack(node.NextLoopPin);
            WriteGotoOutputPin(node.LoopPin);
            builder.AppendLine("}");

            WriteGotoOutputPin(node.CompletedPin);
        }

        public void PureTranslateVariableGetterNode(VariableGetterNode node)
        {
            string valueName = GetOrCreatePinName(node.OutputDataPins[0]);
            
            builder.Append($"{valueName} = ");

            if (node.TargetPin.IncomingPin != null)
            {
                string targetName = GetOrCreatePinName(node.TargetPin.IncomingPin);
                builder.Append($"{targetName}.");
            }
            else
            {
                // Default to this
                builder.Append("this.");
            }

            builder.AppendLine($"{node.VariableName};");
        }

        public void PureTranslateLiteralNode(LiteralNode node)
        {
            string literalString;
            
            // Put quotes around string literals
            if (node.LiteralType == typeof(string))
            {
                literalString = $"\"{node.Value}\"";
            }
            // Put f after float literals
            else if(node.LiteralType == typeof(float))
            {
                literalString = $"{node.Value}f";
            }
            // Put u after uint literals
            else if (node.LiteralType == typeof(uint))
            {
                literalString = $"{node.Value}u";
            }
            // Put single quotes around char literals
            else if(node.LiteralType == typeof(char))
            {
                literalString = $"'{node.Value}'";
            }
            else
            {
                literalString = node.Value.ToString();
            }

            builder.AppendLine($"{GetOrCreatePinName(node.ValuePin)} = {literalString};");
        }
    }
}
