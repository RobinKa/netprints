using NetPrints.Core;
using NetPrints.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetPrints.Translator
{
    public class MethodTranslator
    {
        private const string JumpStackVarName = "jumpStack";
        private const string JumpStackType = "System.Collections.Generic.Stack<int>";

        private Dictionary<NodeOutputDataPin, string> variableNames = new Dictionary<NodeOutputDataPin, string>();
        private Dictionary<Node, List<int>> nodeStateIds = new Dictionary<Node, List<int>>();
        private int nextStateId = 0;
        private IEnumerable<Node> execNodes = new List<Node>();
        private IEnumerable<Node> nodes = new List<Node>();
        private HashSet<NodeInputExecPin> pinsJumpedTo = new HashSet<NodeInputExecPin>();

        private int jumpStackStateId;
        
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
            { typeof(ConstructorNode), new List<NodeTypeHandler> { (translator, node) => translator.TranslateConstructorNode(node as ConstructorNode) } },

            { typeof(ForLoopNode), new List<NodeTypeHandler> {
                (translator, node) => translator.TranslateStartForLoopNode(node as ForLoopNode),
                (translator, node) => translator.TranslateContinueForLoopNode(node as ForLoopNode)} },

            { typeof(VariableGetterNode), new List<NodeTypeHandler> { (translator, node) => translator.PureTranslateVariableGetterNode(node as VariableGetterNode) } },
            { typeof(LiteralNode), new List<NodeTypeHandler> { (translator, node) => translator.PureTranslateLiteralNode(node as LiteralNode) } },
            { typeof(MakeDelegateNode), new List<NodeTypeHandler> { (translator, node) => translator.PureTranslateMakeDelegateNode(node as MakeDelegateNode) } },
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
                if (pin.UsesUnconnectedValue && pin.UnconnectedValue != null)
                {
                    return TranslatorUtil.ObjectToLiteral(pin.UnconnectedValue, (TypeSpecifier)pin.PinType);
                }
                else
                {
                    return $"default({pin.PinType})";
                }
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

        private IEnumerable<string> GetPinIncomingValues(IEnumerable<NodeInputDataPin> pins)
        {
            return pins.Select(pin => GetPinIncomingValue(pin)).ToList();
        }

        private string GetOrCreateTypedPinName(NodeOutputDataPin pin)
        {
            string pinName = GetOrCreatePinName(pin);
            return $"{pin.PinType} {pinName}";
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
                    builder.AppendLine($"{pin.PinType} {variableName};");
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
            else if (method.Modifiers.HasFlag(MethodModifiers.Public))
            {
                builder.Append("public ");
            }
            else if(method.Modifiers.HasFlag(MethodModifiers.Internal))
            {
                builder.Append("internal ");
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

            if(method.Modifiers.HasFlag(MethodModifiers.Override))
            {
                builder.Append("override ");
            }
            else if(method.Modifiers.HasFlag(MethodModifiers.Virtual))
            {
                builder.Append("virtual ");
            }

            // Write return type
            if (method.ReturnTypes.Count() > 1)
            {
                // Tuple<Types..> (won't be needed in the future)
                string returnType = typeof(Tuple).FullName + "<" + string.Join(", ", method.ReturnTypes) + ">";
                builder.Append(returnType + " ");

                //builder.Append($"({string.Join(", ", method.ReturnTypes.Select(t => t.FullName))}) ");
            }
            else if (method.ReturnTypes.Count() == 1)
            {
                builder.Append($"{method.ReturnTypes[0]} ");
            }
            else
            {
                builder.Append("void ");
            }

            // Write name
            builder.Append(method.Name);

            // Write generic arguments if any
            if(method.DeclaredGenericArguments.Count > 0)
            {
                builder.Append("<" + string.Join(", ", method.DeclaredGenericArguments) + ">");
            }

            // Write parameters
            builder.AppendLine($"({string.Join(", ", GetOrCreateTypedPinNames(method.EntryNode.OutputDataPins))})");
        }

        private void TranslateJumpStack()
        {
            builder.AppendLine($"State{jumpStackStateId}:");
            builder.AppendLine($"if({JumpStackVarName}.Count == 0) throw new System.Exception();");
            builder.AppendLine($"switch({JumpStackVarName}.Pop())");
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
            pinsJumpedTo.Clear();
            nextStateId = 0;
            builder.Clear();

            nodes = TranslatorUtil.GetAllNodesInMethod(method);
            execNodes = TranslatorUtil.GetExecNodesInMethod(method);

            // Assign a state id to every non-pure node
            CreateStates();

            // Assign jump stack state id
            // Write it later once we know which states get jumped to
            jumpStackStateId = GetNextStateId();
            
            // Create variables for all output pins for every node
            CreateVariables();

            // Write the signatures
            TranslateSignature();
            builder.AppendLine("{"); // Method start

            // Write a placeholder for the jump stack declaration
            // Replaced later
            builder.Append("%JUMPSTACKPLACEHOLDER%");

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

            // Write the jump stack if it was ever used
            if (pinsJumpedTo.Count > 0)
            {
                TranslateJumpStack();
                
                builder.Replace("%JUMPSTACKPLACEHOLDER%", $"{JumpStackType} {JumpStackVarName} = new {JumpStackType}();{Environment.NewLine}");
            }
            else
            {
                builder.Replace("%JUMPSTACKPLACEHOLDER%", "");
            }
            
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

        private void WriteGotoJumpStack()
        {
            builder.AppendLine($"goto State{jumpStackStateId};");
        }

        private void WritePushJumpStack(NodeInputExecPin pin)
        {
            if (!pinsJumpedTo.Contains(pin))
            {
                pinsJumpedTo.Add(pin);
            }

            builder.AppendLine($"{JumpStackVarName}.Push({GetExecPinStateId(pin)});");
        }

        private void WriteGotoInputPin(NodeInputExecPin pin)
        {
            builder.AppendLine($"goto State{GetExecPinStateId(pin)};");
        }

        private void WriteGotoOutputPin(NodeOutputExecPin pin)
        {
            if(pin.OutgoingPin == null)
            {
                WriteGotoJumpStack();
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
            string temporaryReturnName = null;

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
                temporaryReturnName = TranslatorUtil.GetTemporaryVariableName();

                var returnTypeNames = string.Join(", ", node.OutputDataPins.Select(pin => pin.PinType));
                
                builder.Append($"{typeof(Tuple).FullName}<{returnTypeNames}> {temporaryReturnName} = ");
            }

            // Static: Write class name / target, default to own class name
            // Instance: Write target, default to this

            if (node.IsStatic)
            {
                builder.Append($"{node.DeclaringType}.");
            }
            else
            {
                if (node.TargetPin.IncomingPin != null)
                {
                    string targetName = GetOrCreatePinName(node.TargetPin.IncomingPin);
                    builder.Append($"{targetName}.");
                }
                else
                {
                    // Default to thise
                    builder.Append("this.");
                }
            }

            // Write function call with arguments
            var argumentNames = GetPinIncomingValues(node.ArgumentPins);

            builder.AppendLine($"{node.MethodName}({string.Join(", ", argumentNames)});");

            // Assign the real variables from the temporary tuple
            if(node.OutputDataPins.Count > 1)
            {
                var returnNames = GetOrCreatePinNames(node.OutputDataPins);
                for(int i = 0; i < returnNames.Count(); i++)
                {
                    builder.AppendLine($"{returnNames.ElementAt(i)} = {temporaryReturnName}.Item{i+1};");
                }
            }

            // Go to the next state
            WriteGotoOutputPin(node.OutputExecPins[0]);
        }

        public void TranslateConstructorNode(ConstructorNode node)
        {
            // Translate all the pure nodes this node depends on in
            // the correct order
            TranslateDependentPureNodes(node);

            // Write assignment and constructor
            string returnName = GetOrCreatePinName(node.OutputDataPins[0]);
            builder.Append($"{returnName} = new {node.ClassType}");
            
            // Write constructor arguments
            var argumentNames = GetPinIncomingValues(node.ArgumentPins);
            builder.AppendLine($"({string.Join(", ", argumentNames)});");

            // Go to the next state
            WriteGotoOutputPin(node.OutputExecPins[0]);
        }

        public void TranslateVariableSetterNode(VariableSetterNode node)
        {
            // Translate all the pure nodes this node depends on in
            // the correct order
            TranslateDependentPureNodes(node);
            
            string valueName = GetOrCreatePinName(node.InputDataPins[1].IncomingPin);

            // Add target name or this if not a local variable
            if (!node.IsLocalVariable)
            {
                if (node.InputDataPins[0].IncomingPin != null)
                {
                    string targetName = GetOrCreatePinName(node.InputDataPins[0].IncomingPin);
                    builder.Append($"{targetName}.");
                }
                else
                {
                    builder.Append("this.");
                }
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
                string returnType = typeof(Tuple).FullName + "<" + string.Join(", ", node.InputDataPins.Select(pin => pin.PinType)) + ">";
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
            WritePushJumpStack(node.ContinuePin);
            WriteGotoOutputPin(node.LoopPin);
            builder.AppendLine("}");
        }

        public void TranslateContinueForLoopNode(ForLoopNode node)
        {
            // Translate all the pure nodes this node depends on in
            // the correct order
            TranslateDependentPureNodes(node);

            builder.AppendLine($"{GetOrCreatePinName(node.IndexPin)}++;");
            builder.AppendLine($"if({GetOrCreatePinName(node.IndexPin)} < {GetPinIncomingValue(node.MaxIndexPin)})");
            builder.AppendLine("{");
            WritePushJumpStack(node.ContinuePin);
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
            string literalString = TranslatorUtil.ObjectToLiteral(node.Value, node.LiteralType);

            builder.AppendLine($"{GetOrCreatePinName(node.ValuePin)} = {literalString};");
        }

        public void PureTranslateMakeDelegateNode(MakeDelegateNode node)
        {
            // Write assignment of return value
            string returnName = GetOrCreatePinName(node.OutputDataPins[0]);
            builder.Append($"{returnName} = ");

            // Static: Write class name / target, default to own class name
            // Instance: Write target, default to this

            if (node.IsFromStaticMethod)
            {
                builder.Append($"{node.MethodSpecifier.DeclaringType}.");
            }
            else
            {
                if (node.TargetPin.IncomingPin != null)
                {
                    string targetName = GetOrCreatePinName(node.TargetPin.IncomingPin);
                    builder.Append($"{targetName}.");
                }
                else
                {
                    // Default to thise
                    builder.Append("this.");
                }
            }

            // Write method name
            builder.AppendLine($"{node.MethodSpecifier.Name};");
        }
    }
}
