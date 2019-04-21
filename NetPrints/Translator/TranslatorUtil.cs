using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using NetPrints.Core;
using NetPrints.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NetPrints.Translator
{
    public static class TranslatorUtil
    {
        public const string VariablePrefix = "var";
        public const string TemporaryVariablePrefix = "temp";

        public readonly static Dictionary<MemberVisibility, string> VisibilityTokens = new Dictionary<MemberVisibility, string>()
        {
            [MemberVisibility.Private] = "private",
            [MemberVisibility.Protected] = "protected",
            [MemberVisibility.Public] = "public",
            [MemberVisibility.Internal] = "internal",
        };

        private const int TemporaryVariableNameLength = 16;

        /// <summary>
        /// Gets a temporary variable name.
        /// </summary>
        /// <returns>Temporary variable name.</returns>
        public static string GetTemporaryVariableName(Random random)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";

            string name = TemporaryVariablePrefix;
            int additionalLength = TemporaryVariableNameLength - name.Length;
            if (additionalLength > 0)
            {
                string addition = new string(Enumerable.Repeat(chars, additionalLength).Select(s => s[random.Next(s.Length)]).ToArray());
                addition = addition[0].ToString().ToUpper() + string.Join("", addition.Skip(1));
                name += addition;
            }

            return name;
        }

        /// <summary>
        /// Translates an object into a literal value (eg. a float 32.32 -> "32.32f")
        /// </summary>
        /// <param name="obj">Object value to translate.</param>
        /// <param name="type">Specifier for the type of the literal.</param>
        /// <returns></returns>
        public static string ObjectToLiteral(object obj, TypeSpecifier type)
        {
            // Interpret object string as enum field
            if (type.IsEnum)
            {
                return $"{type}.{obj}";
            }
            // Put quotes around string literals
            if (type == TypeSpecifier.FromType<string>())
            {
                return $"\"{obj}\"";
            }
            else if (type == TypeSpecifier.FromType<float>())
            {
                return $"{obj}F";
            }
            else if (type == TypeSpecifier.FromType<double>())
            {
                return $"{obj}D";
            }
            else if (type == TypeSpecifier.FromType<uint>())
            {
                return $"{obj}U";
            }
            // Put single quotes around char literals
            else if (type == TypeSpecifier.FromType<char>())
            {
                return $"'{obj}'";
            }
            else if (type == TypeSpecifier.FromType<long>())
            {
                return $"{obj}L";
            }
            else if (type == TypeSpecifier.FromType<ulong>())
            {
                return $"{obj}UL";
            }
            else if (type == TypeSpecifier.FromType<decimal>())
            {
                return $"{obj}M";
            }
            else
            {
                return obj.ToString();
            }
        }

        /// <summary>
        /// Returns the first name not already contained in a list of names by
        /// trying to generate a unique name based on the given name.
        /// Includes a prefix in front of the name.
        /// </summary>
        /// <param name="name">Name to make unique.</param>
        /// <param name="names">List of names already existing.</param>
        /// <returns>Name based on name but not contained in names.</returns>
        public static string GetUniqueVariableName(string name, IList<string> names)
        {
            // Don't allow illegal characters in the name
            // TODO: Make this more general
            name = name.Replace("+", "_").Replace("[", "").Replace("]", "Array").Replace(",", "");

            int i = 1;

            while(true)
            {
                string uniqueName = i == 1 ? $"{VariablePrefix}{name}" : $"{VariablePrefix}{name}{i}";

                if (!names.Contains(uniqueName))
                {
                    return uniqueName;
                }

                i++;
            }
        }

        private static void AddAllNodes(Node node, ref HashSet<Node> nodes)
        {
            nodes.Add(node);

            foreach (NodeOutputExecPin pin in node.OutputExecPins)
            {
                if (pin.OutgoingPin != null && !nodes.Contains(pin.OutgoingPin.Node))
                {
                    AddAllNodes(pin.OutgoingPin.Node, ref nodes);
                }
            }

            foreach (NodeInputDataPin pin in node.InputDataPins)
            {
                if (pin.IncomingPin != null && !nodes.Contains(pin.IncomingPin.Node))
                {
                    AddAllNodes(pin.IncomingPin.Node, ref nodes);
                }
            }

            foreach (NodeInputTypePin pin in node.InputTypePins)
            {
                if (pin.IncomingPin != null && !nodes.Contains(pin.IncomingPin.Node))
                {
                    AddAllNodes(pin.IncomingPin.Node, ref nodes);
                }
            }
        }

        /// <summary>
        /// Gets all nodes contained in a graph.
        /// </summary>
        /// <param name="graph">Graph containing the nodes.</param>
        /// <returns>Nodes contained in the graph.</returns>
        public static IEnumerable<Node> GetAllNodesInExecGraph(ExecutionGraph graph)
        {
            HashSet<Node> nodes = new HashSet<Node>();

            AddAllNodes(graph.EntryNode, ref nodes);

            return nodes;
        }

        private static void AddExecNodes(Node node, ref HashSet<Node> nodes)
        {
            nodes.Add(node);

            foreach (NodeOutputExecPin pin in node.OutputExecPins)
            {
                if (pin.OutgoingPin != null && !nodes.Contains(pin.OutgoingPin.Node))
                {
                    AddExecNodes(pin.OutgoingPin.Node, ref nodes);
                }
            }
        }

        /// <summary>
        /// Gets all execution contained nodes in a graph
        /// </summary>
        /// <param name="graph">Graph containing the execution nodes.</param>
        /// <returns>Execution nodes contained in the graph.</returns>
        public static IEnumerable<Node> GetExecNodesInExecGraph(ExecutionGraph graph)
        {
            HashSet<Node> nodes = new HashSet<Node>();

            AddExecNodes(graph.EntryNode, ref nodes);

            return nodes;
        }

        private static void AddDependentPureNodes(Node node, ref HashSet<Node> nodes)
        {
            // Only add pure nodes (the initial node might not be pure)
            if (node.IsPure)
            {
                nodes.Add(node);
            }

            foreach (NodeInputDataPin pin in node.InputDataPins)
            {
                if (pin.IncomingPin?.Node.IsPure == true && !nodes.Contains(pin.IncomingPin.Node))
                {
                    AddDependentPureNodes(pin.IncomingPin.Node, ref nodes);
                }
            }
        }

        /// <summary>
        /// Gets all pure nodes a node depends on.
        /// </summary>
        /// <param name="node">Node whose dependent pure nodes to get.</param>
        /// <returns>Pure nodes the node depends on.</returns>
        public static IEnumerable<Node> GetDependentPureNodes(Node node)
        {
            HashSet<Node> nodes = new HashSet<Node>();

            AddDependentPureNodes(node, ref nodes);

            return nodes;
        }

        /// <summary>
        /// Gets all pure nodes a node depends on sorted by depth.
        /// </summary>
        /// <param name="node">Node whose dependent pure nodes to get.</param>
        /// <returns>Pure nodes the node depends on sorted by depth.</returns>
        public static IEnumerable<Node> GetSortedPureNodes(Node node)
        {
            var dependentNodes = GetDependentPureNodes(node);
            List<Node> remainingNodes = new List<Node>(dependentNodes);
            List<Node> sortedNodes = new List<Node>();

            List<Node> newNodes = new List<Node>();

            do
            {
                newNodes.Clear();

                foreach (Node evalNode in remainingNodes)
                {
                    // Check whether all of this node's dependencies have been evaluated
                    if (evalNode.InputDataPins.All(inNode => inNode.IncomingPin?.Node.IsPure != true || sortedNodes.Contains(inNode.IncomingPin.Node)))
                    {
                        newNodes.Add(evalNode);
                    }
                }

                // Add newly found nodes
                foreach (Node newNode in newNodes)
                {
                    remainingNodes.Remove(newNode);
                }

                sortedNodes.AddRange(newNodes);
            }
            while (newNodes.Count > 0 && remainingNodes.Count > 0);

            Debug.Assert(remainingNodes.Count == 0, "Impossible to evaluate all nodes (cyclic dependencies?)");

            return sortedNodes;
        }

        public static string FormatCode(string code)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            SyntaxNode formatted = Formatter.Format(syntaxTree.GetCompilationUnitRoot(), new AdhocWorkspace()).NormalizeWhitespace();
            return formatted.ToFullString();
        }
    }
}
