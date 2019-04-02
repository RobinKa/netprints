using NetPrints.Core;
using NetPrints.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetPrints.Translator
{
    /// <summary>
    /// Translates methods into C#.
    /// </summary>
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

        private Random random;

        private delegate void NodeTypeHandler(MethodTranslator translator, Node node);

        private Dictionary<Type, List<NodeTypeHandler>> nodeTypeHandlers = new Dictionary<Type, List<NodeTypeHandler>>()
        {
            { typeof(CallMethodNode), new List<NodeTypeHandler> { (translator, node) => translator.TranslateCallMethodNode(node as CallMethodNode) } },
            { typeof(VariableSetterNode), new List<NodeTypeHandler> { (translator, node) => translator.TranslateVariableSetterNode(node as VariableSetterNode) } },
            { typeof(ReturnNode), new List<NodeTypeHandler> { (translator, node) => translator.TranslateReturnNode(node as ReturnNode) } },
            { typeof(EntryNode), new List<NodeTypeHandler> { (translator, node) => translator.TranslateMethodEntry(node as EntryNode) } },
            { typeof(IfElseNode), new List<NodeTypeHandler> { (translator, node) => translator.TranslateIfElseNode(node as IfElseNode) } },
            { typeof(ConstructorNode), new List<NodeTypeHandler> { (translator, node) => translator.TranslateConstructorNode(node as ConstructorNode) } },
            { typeof(ExplicitCastNode), new List<NodeTypeHandler> { (translator, node) => translator.TranslateExplicitCastNode(node as ExplicitCastNode) } },

            { typeof(ForLoopNode), new List<NodeTypeHandler> {
                (translator, node) => translator.TranslateStartForLoopNode(node as ForLoopNode),
                (translator, node) => translator.TranslateContinueForLoopNode(node as ForLoopNode)} },

            { typeof(RerouteNode), new List<NodeTypeHandler> { (translator, node) => translator.TranslateRerouteNode(node as RerouteNode) } },

            { typeof(VariableGetterNode), new List<NodeTypeHandler> { (translator, node) => translator.PureTranslateVariableGetterNode(node as VariableGetterNode) } },
            { typeof(LiteralNode), new List<NodeTypeHandler> { (translator, node) => translator.PureTranslateLiteralNode(node as LiteralNode) } },
            { typeof(MakeDelegateNode), new List<NodeTypeHandler> { (translator, node) => translator.PureTranslateMakeDelegateNode(node as MakeDelegateNode) } },
            { typeof(TypeOfNode), new List<NodeTypeHandler> { (translator, node) => translator.PureTranslateTypeOfNode(node as TypeOfNode) } },
            { typeof(MakeArrayNode), new List<NodeTypeHandler> { (translator, node) => translator.PureTranslateMakeArrayNode(node as MakeArrayNode) } },
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


            string pinName;

            // Special case for property setters, input name "value".
            // TODO: Don't rely on set_ prefix
            if (pin.Node is EntryNode && method.Name.StartsWith("set_"))
            {
                pinName = "value";
            }
            else
            {
                pinName = TranslatorUtil.GetUniqueVariableName(pin.Name.Replace("<", "_").Replace(">", "_"), variableNames.Values.ToList());
            }

            variableNames.Add(pin, pinName);
            return pinName;
        }

        private string GetPinIncomingValue(NodeInputDataPin pin)
        {
            if(pin.IncomingPin == null)
            {
                if (pin.UsesUnconnectedValue && pin.UnconnectedValue != null)
                {
                    return TranslatorUtil.ObjectToLiteral(pin.UnconnectedValue, (TypeSpecifier)pin.PinType.Value);
                }
                else
                {
                    return $"default({pin.PinType.Value.FullCodeName})";
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
            return $"{pin.PinType.Value.FullCodeName} {pinName}";
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
            builder.AppendLine("// Variables");

            foreach (var v in variableNames)
            {
                NodeOutputDataPin pin = v.Key;
                string variableName = v.Value;

                if (!(pin.Node is EntryNode))
                {
                    builder.AppendLine($"{pin.PinType.Value.FullCodeName} {variableName} = default({pin.PinType.Value.FullCodeName});");
                }
            }
        }

        private void TranslateSignature()
        {
            builder.AppendLine($"// Method {method.Name}");

            // Write visibility
            builder.Append($"{TranslatorUtil.VisibilityTokens[method.Visibility]} ");

            // Write modifiers
            if (method.Modifiers.HasFlag(MethodModifiers.Static))
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
                string returnType = typeof(Tuple).FullName + "<" + string.Join(", ", method.ReturnTypes.Select(t => t.FullCodeName)) + ">";
                builder.Append(returnType + " ");

                //builder.Append($"({string.Join(", ", method.ReturnTypes.Select(t => t.FullName))}) ");
            }
            else if (method.ReturnTypes.Count() == 1)
            {
                builder.Append($"{method.ReturnTypes.Single().FullCodeName} ");
            }
            else
            {
                builder.Append("void ");
            }

            // Write name
            builder.Append(method.Name);

            // Write generic arguments if any
            if(method.GenericArgumentTypes.Any())
            {
                builder.Append("<" + string.Join(", ", method.GenericArgumentTypes.Select(arg => arg.FullCodeName)) + ">");
            }

            // Write parameters
            builder.AppendLine($"({string.Join(", ", GetOrCreateTypedPinNames(method.EntryNode.OutputDataPins))})");
        }

        private void TranslateJumpStack()
        {
            builder.AppendLine("// Jump stack");

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

        /// <summary>
        /// Translates a method to C#.
        /// </summary>
        /// <param name="method">Method to translate.</param>
        /// <param name="withSignature">Whether to translate the signature.</param>
        /// <returns>C# code for the method.</returns>
        public string Translate(Method method, bool withSignature)
        {
            this.method = method;

            // Reset state
            variableNames.Clear();
            nodeStateIds.Clear();
            pinsJumpedTo.Clear();
            nextStateId = 0;
            builder.Clear();
            random = new Random(0);

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
            if (withSignature)
            {
                TranslateSignature();
            }

            builder.AppendLine("{"); // Method start

            // Write a placeholder for the jump stack declaration
            // Replaced later
            builder.Append("%JUMPSTACKPLACEHOLDER%");

            // Write the variable declarations
            TranslateVariables();
            builder.AppendLine();

            // Start at node after method entry if necessary (id!=0)
            if (method.EntryNode.OutputExecPins[0].OutgoingPin != null && GetExecPinStateId(method.EntryNode.OutputExecPins[0].OutgoingPin) != 0)
            {
                WriteGotoOutputPin(method.EntryNode.OutputExecPins[0]);
            }
            
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

            string code = builder.ToString();

            // Remove unused labels
            code = RemoveUnnecessaryLabels(code);

            return code;
        }

        private string RemoveUnnecessaryLabels(string code)
        {
            foreach (int stateId in nodeStateIds.Values.SelectMany(i => i))
            {
                if (!code.Contains($"goto State{stateId};"))
                {
                    code = code.Replace($"State{stateId}:", "");
                }
            }

            return code;
        }

        public void TranslateNode(Node node, int pinIndex)
        {
            if (!(node is RerouteNode))
            {
                builder.AppendLine($"// {node.ToString()}");
            }

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

        private void WriteGotoOutputPinIfNecessary(NodeOutputExecPin pin, NodeInputExecPin fromPin)
        {
            int fromId = GetExecPinStateId(fromPin);
            int nextId = fromId + 1;

            if (pin.OutgoingPin == null)
            {
                if (nextId != jumpStackStateId)
                {
                    WriteGotoJumpStack();
                }
            }
            else
            {
                
                int toId = GetExecPinStateId(pin.OutgoingPin);

                // Only write the goto if the next state is not
                // the state we want to go to.
                if (nextId != toId)
                {
                    WriteGotoInputPin(pin.OutgoingPin);
                }
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
            /*// Go to the next state.
            // Only write if it's not the initial state (id==0) anyway.
            if (node.OutputExecPins[0].OutgoingPin != null && GetExecPinStateId(node.OutputExecPins[0].OutgoingPin) != 0)
            {
                WriteGotoOutputPin(node.OutputExecPins[0]);
            }*/
        }
        
        public void TranslateCallMethodNode(CallMethodNode node)
        {
            // Wrap in try / catch
            if (node.HandlesExceptions)
            {
                builder.AppendLine("try");
                builder.AppendLine("{");
            }

            string temporaryReturnName = null;

            if (!node.IsPure)
            {
                // Translate all the pure nodes this node depends on in
                // the correct order
                TranslateDependentPureNodes(node);
            }

            // Write assignment of return values
            if (node.ReturnValuePins.Count == 1)
            {
                string returnName = GetOrCreatePinName(node.ReturnValuePins[0]);

                builder.Append($"{returnName} = ");
            }
            else if (node.OutputDataPins.Count > 1)
            {
                temporaryReturnName = TranslatorUtil.GetTemporaryVariableName(random);

                var returnTypeNames = string.Join(", ", node.OutputDataPins.Select(pin => pin.PinType.Value.FullCodeName));
                
                builder.Append($"{typeof(Tuple).FullName}<{returnTypeNames}> {temporaryReturnName} = ");
            }

            // Get arguments for method call
            var argumentNames = GetPinIncomingValues(node.ArgumentPins);

            // Check whether the method is an operator and we need to translate its name
            // into operator symbols. Otherwise just call the method normally.
            if (OperatorUtil.TryGetOperatorInfo(node.MethodSpecifier, out OperatorInfo operatorInfo))
            {
                if (operatorInfo.Unary)
                {
                    if (argumentNames.Count() != 1)
                    {
                        throw new Exception($"Unary operator was found but did not have one argument: {node.MethodName}");
                    }

                    if (operatorInfo.UnaryRightPosition)
                    {
                        builder.AppendLine($"{argumentNames.ElementAt(0)}{operatorInfo.Symbol};");
                    }
                    else
                    {
                        builder.AppendLine($"{operatorInfo.Symbol}{argumentNames.ElementAt(0)};");
                    }
                }
                else
                {
                    if (argumentNames.Count() != 2)
                    {
                        throw new Exception($"Binary operator was found but did not have two arguments: {node.MethodName}");
                    }

                    builder.AppendLine($"{argumentNames.ElementAt(0)}{operatorInfo.Symbol}{argumentNames.ElementAt(1)};");
                }
            }
            else
            {
                // Static: Write class name / target, default to own class name
                // Instance: Write target, default to this

                if (node.IsStatic)
                {
                    builder.Append($"{node.DeclaringType.FullCodeName}.");
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
                        // Default to this
                        builder.Append("this.");
                    }
                }

                // Prefix with "out" / "ref" / "in"
                string[] argNameArray = argumentNames.ToArray();
                Debug.Assert(argNameArray.Length == node.MethodSpecifier.Parameters.Count);

                for (int i = 0; i < node.MethodSpecifier.Parameters.Count; i++)
                {
                    MethodParameterPassType passType = node.MethodSpecifier.Parameters[i].PassType;
                    switch (passType)
                    {
                        case MethodParameterPassType.Out:
                            argNameArray[i] = "out " + argNameArray[i];
                            break;
                        case MethodParameterPassType.Reference:
                            argNameArray[i] = "ref " + argNameArray[i];
                            break;
                        case MethodParameterPassType.In:
                            // Don't pass with in as it could break implicit casts.
                            // argNameArray[i] = "in " + argNameArray[i];
                            break;
                        default:
                            break;
                    }
                }

                // Write the method call
                builder.AppendLine($"{node.BoundMethodName}({string.Join(", ", argNameArray)});");
            }

            // Assign the real variables from the temporary tuple
            if(node.ReturnValuePins.Count > 1)
            {
                var returnNames = GetOrCreatePinNames(node.ReturnValuePins);
                for(int i = 0; i < returnNames.Count(); i++)
                {
                    builder.AppendLine($"{returnNames.ElementAt(i)} = {temporaryReturnName}.Item{i+1};");
                }
            }

            // Set the exception to null on success if catch pin is connected
            if (node.HandlesExceptions)
            {
                builder.AppendLine($"{GetOrCreatePinName(node.ExceptionPin)} = null;");
            }

            // Go to the next state
            if (!node.IsPure)
            {
                WriteGotoOutputPinIfNecessary(node.OutputExecPins[0], node.InputExecPins[0]);
            }

            // Catch exceptions if catch pin is connected
            if (node.HandlesExceptions)
            {
                string exceptionVarName = TranslatorUtil.GetTemporaryVariableName(random);
                builder.AppendLine("}");
                builder.AppendLine($"catch (System.Exception {exceptionVarName})");
                builder.AppendLine("{");
                builder.AppendLine($"{GetOrCreatePinName(node.ExceptionPin)} = {exceptionVarName};");

                // Set all return values to default on exception
                foreach (var returnValuePin in node.ReturnValuePins)
                {
                    string returnName = GetOrCreatePinName(returnValuePin);
                    builder.AppendLine($"{returnName} = default({returnValuePin.PinType.Value.FullCodeName});");
                }

                if (!node.IsPure)
                {
                    WriteGotoOutputPinIfNecessary(node.CatchPin, node.InputExecPins[0]);
                }

                builder.AppendLine("}");
            }
        }

        public void TranslateConstructorNode(ConstructorNode node)
        {
            if (!node.IsPure)
            {
                // Translate all the pure nodes this node depends on in
                // the correct order
                TranslateDependentPureNodes(node);
            }

            // Write assignment and constructor
            string returnName = GetOrCreatePinName(node.OutputDataPins[0]);
            builder.Append($"{returnName} = new {node.ClassType}");
            
            // Write constructor arguments
            var argumentNames = GetPinIncomingValues(node.ArgumentPins);
            builder.AppendLine($"({string.Join(", ", argumentNames)});");

            if (!node.IsPure)
            {
                // Go to the next state
                WriteGotoOutputPinIfNecessary(node.OutputExecPins[0], node.InputExecPins[0]);
            }
        }

        public void TranslateExplicitCastNode(ExplicitCastNode node)
        {
            if (!node.IsPure)
            {
                // Translate all the pure nodes this node depends on in
                // the correct order
                TranslateDependentPureNodes(node);
            }

            // Try to cast the incoming object and go to next states.
            if (node.ObjectToCast.IncomingPin != null)
            {
                string pinToCastName = GetPinIncomingValue(node.ObjectToCast);
                string outputName = GetOrCreatePinName(node.CastPin);

                // If failure pin is not connected write explicit cast that throws.
                // Otherwise check if cast object is null and execute failure
                // path if it is.
                if (node.IsPure || node.CastFailedPin.OutgoingPin == null)
                {
                    builder.AppendLine($"{outputName} = ({node.CastType.FullCodeNameUnbound}){pinToCastName};");

                    if (!node.IsPure)
                    {
                        WriteGotoOutputPinIfNecessary(node.CastSuccessPin, node.InputExecPins[0]);
                    }
                }
                else
                {
                    builder.AppendLine($"{outputName} = {pinToCastName} as {node.CastType.FullCodeNameUnbound};");

                    if (!node.IsPure)
                    {
                        builder.AppendLine($"if ({outputName} is null)");
                        builder.AppendLine("{");
                        WriteGotoOutputPinIfNecessary(node.CastFailedPin, node.InputExecPins[0]);
                        builder.AppendLine("}");
                        builder.AppendLine("else");
                        builder.AppendLine("{");
                        WriteGotoOutputPinIfNecessary(node.CastSuccessPin, node.InputExecPins[0]);
                        builder.AppendLine("}");
                    }
                }
            }
        }

        public void TranslateVariableSetterNode(VariableSetterNode node)
        {
            // Translate all the pure nodes this node depends on in
            // the correct order
            TranslateDependentPureNodes(node);
            
            string valueName = GetPinIncomingValue(node.NewValuePin);

            // Add target name if there is a target (null for local and static variables)
            if (node.IsStatic)
            {
                if (!(node.TargetType is null))
                {
                    builder.Append(node.TargetType.FullCodeName);
                }
                else
                {
                    builder.Append(node.Method.Class.Name);
                }
            }
            if (node.TargetPin != null)
            {
                if (node.TargetPin.IncomingPin != null)
                {
                    string targetName = GetOrCreatePinName(node.TargetPin.IncomingPin);
                    builder.Append(targetName);
                }
                else
                {
                    builder.Append("this");
                }
            }

            // Add index if needed
            if (node.IsIndexer)
            {
                builder.Append($"[{GetPinIncomingValue(node.IndexPin)}]");
            }
            else
            {
                builder.Append($".{node.VariableName}");
            }

            builder.AppendLine($" = {valueName};");

            // Set output pin of this node to the same value
            builder.AppendLine($"{GetOrCreatePinName(node.OutputDataPins[0])} = {valueName};");

            // Go to the next state
            WriteGotoOutputPinIfNecessary(node.OutputExecPins[0], node.InputExecPins[0]);
        }

        public void TranslateReturnNode(ReturnNode node)
        {
            // Translate all the pure nodes this node depends on in
            // the correct order
            TranslateDependentPureNodes(node);

            if (node.InputDataPins.Count == 0)
            {
                // Only write return if the return node is not the last node
                if (GetExecPinStateId(node.InputExecPins[0]) != nodeStateIds.Count - 1)
                {
                    builder.AppendLine("return;");
                }
            }
            else if(node.InputDataPins.Count == 1)
            {
                builder.AppendLine($"return {GetPinIncomingValue(node.InputDataPins[0])};");
            }
            else
            {
                var returnValues = node.InputDataPins.Select(pin => GetPinIncomingValue(pin));

                // Tuple<Types..> (won't be needed in the future)
                string returnType = typeof(Tuple).FullName + "<" + string.Join(", ", node.InputDataPins.Select(pin => pin.PinType.Value.FullCodeName)) + ">";
                builder.AppendLine($"return new {returnType}({string.Join(", ", returnValues)});");
            }
        }

        public void TranslateIfElseNode(IfElseNode node)
        {
            // Translate all the pure nodes this node depends on in
            // the correct order
            TranslateDependentPureNodes(node);

            string conditionVar = GetPinIncomingValue(node.ConditionPin);

            builder.AppendLine($"if ({conditionVar})");
            builder.AppendLine("{");

            if (node.TruePin.OutgoingPin != null)
            {
                WriteGotoOutputPinIfNecessary(node.TruePin, node.InputExecPins[0]);
            }
            else
            {
                builder.AppendLine("return;");
            }

            builder.AppendLine("}");

            builder.AppendLine("else");
            builder.AppendLine("{");

            if (node.FalsePin.OutgoingPin != null)
            {
                WriteGotoOutputPinIfNecessary(node.FalsePin, node.InputExecPins[0]);
            }
            else
            {
                builder.AppendLine("return;");
            }

            builder.AppendLine("}");
        }

        public void TranslateStartForLoopNode(ForLoopNode node)
        {
            // Translate all the pure nodes this node depends on in
            // the correct order
            TranslateDependentPureNodes(node);
            
            builder.AppendLine($"{GetOrCreatePinName(node.IndexPin)} = {GetPinIncomingValue(node.InitialIndexPin)};");
            builder.AppendLine($"if ({GetOrCreatePinName(node.IndexPin)} < {GetPinIncomingValue(node.MaxIndexPin)})");
            builder.AppendLine("{");
            WritePushJumpStack(node.ContinuePin);
            WriteGotoOutputPinIfNecessary(node.LoopPin, node.ExecutionPin);
            builder.AppendLine("}");
        }

        public void TranslateContinueForLoopNode(ForLoopNode node)
        {
            // Translate all the pure nodes this node depends on in
            // the correct order
            TranslateDependentPureNodes(node);

            builder.AppendLine($"{GetOrCreatePinName(node.IndexPin)}++;");
            builder.AppendLine($"if ({GetOrCreatePinName(node.IndexPin)} < {GetPinIncomingValue(node.MaxIndexPin)})");
            builder.AppendLine("{");
            WritePushJumpStack(node.ContinuePin);
            WriteGotoOutputPinIfNecessary(node.LoopPin, node.ContinuePin);
            builder.AppendLine("}");

            WriteGotoOutputPinIfNecessary(node.CompletedPin, node.ContinuePin);
        }

        public void PureTranslateVariableGetterNode(VariableGetterNode node)
        {
            string valueName = GetOrCreatePinName(node.OutputDataPins[0]);
            
            builder.Append($"{valueName} = ");

            if (node.IsStatic)
            {
                if (!(node.TargetType is null))
                {
                    builder.Append(node.TargetType.FullCodeName);
                }
                else
                {
                    builder.Append(node.Method.Class.Name);
                }
            }
            else
            {
                if (node.TargetPin != null && node.TargetPin.IncomingPin != null)
                {
                    string targetName = GetOrCreatePinName(node.TargetPin.IncomingPin);
                    builder.Append(targetName);
                }
                else
                {
                    // Default to this
                    builder.Append("this");
                }
            }

            // Add index if needed
            if (node.IsIndexer)
            {
                builder.Append($"[{GetPinIncomingValue(node.IndexPin)}]");
            }
            else
            {
                builder.Append($".{node.VariableName}");
            }

            builder.AppendLine(";");
        }

        public void PureTranslateLiteralNode(LiteralNode node)
        {
            builder.AppendLine($"{GetOrCreatePinName(node.ValuePin)} = {GetPinIncomingValue(node.InputDataPins[0])};");
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

        public void PureTranslateTypeOfNode(TypeOfNode node)
        {
            builder.AppendLine($"{GetOrCreatePinName(node.TypePin)} = typeof({node.InputTypePin.InferredType?.Value?.FullCodeNameUnbound ?? "System.Object"});");
        }

        public void PureTranslateMakeArrayNode(MakeArrayNode node)
        {
            builder.Append($"{GetOrCreatePinName(node.OutputDataPins[0])} = new {node.ArrayType.FullCodeName}");

            // Use predefined size or initializer list
            if (node.UsePredefinedSize)
            {
                // HACKish: Remove trailing "[]" contained in type
                builder.Remove(builder.Length - 2, 2);
                builder.AppendLine($"[{GetPinIncomingValue(node.SizePin)}];");
            }
            else
            {
                builder.AppendLine();
                builder.AppendLine("{");

                foreach (var inputDataPin in node.InputDataPins)
                {
                    builder.AppendLine($"{GetPinIncomingValue(inputDataPin)},");
                }

                builder.AppendLine("};");
            }
        }

        public void TranslateRerouteNode(RerouteNode node)
        {
            if (node.ExecRerouteCount + node.TypeRerouteCount + node.DataRerouteCount != 1)
            {
                throw new NotImplementedException("Only implemented reroute nodes with exactly 1 type of pin.");
            }

            if (node.DataRerouteCount == 1)
            {
                builder.AppendLine($"{GetOrCreatePinName(node.OutputDataPins[0])} = {GetPinIncomingValue(node.InputDataPins[0])};");
            }
            else if (node.ExecRerouteCount == 1)
            {
                WriteGotoOutputPinIfNecessary(node.OutputExecPins[0], node.InputExecPins[0]);
            }
        }
    }
}
