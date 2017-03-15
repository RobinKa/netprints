using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using NetPrints.Core;
using NetPrints.Graph;

namespace NetPrints.Translator
{
    public static class TranslatorUtil
    {
        public const string VariablePrefix = "var";
        public const string TemporaryVariablePrefix = "temp";

        private static Random random = new Random();

        private const int TemporaryVariableNameLength = 16;

        public static string GetTemporaryVariableName()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";

            string name = TemporaryVariablePrefix;
            int additionalLength = TemporaryVariableNameLength - name.Length;
            if(additionalLength > 0)
            {
                string addition = new string(Enumerable.Repeat(chars, additionalLength).Select(s => s[random.Next(s.Length)]).ToArray());
                addition = addition.First().ToString().ToUpper() + string.Join("", addition.Skip(1));
                name += addition;
            }

            return name;
        }

        public static string GetUniqueVariableName(string name, IList<string> names)
        {
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

            foreach(NodeOutputExecPin pin in node.OutputExecPins)
            {
                if(pin.OutgoingPin != null && !nodes.Contains(pin.OutgoingPin.Node))
                {
                    AddAllNodes(pin.OutgoingPin.Node, ref nodes);
                }
            }

            foreach(NodeInputDataPin pin in node.InputDataPins)
            {
                if(pin.IncomingPin != null && !nodes.Contains(pin.IncomingPin.Node))
                {
                    AddAllNodes(pin.IncomingPin.Node, ref nodes);
                }
            }
        }

        public static IEnumerable<Node> GetAllNodesInMethod(Method method)
        {
            HashSet<Node> nodes = new HashSet<Node>();

            AddAllNodes(method.EntryNode, ref nodes);

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

        public static IEnumerable<Node> GetExecNodesInMethod(Method method)
        {
            HashSet<Node> nodes = new HashSet<Node>();

            AddExecNodes(method.EntryNode, ref nodes);

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
                if (pin.IncomingPin != null && pin.IncomingPin.Node.IsPure && !nodes.Contains(pin.IncomingPin.Node))
                {
                    AddDependentPureNodes(pin.IncomingPin.Node, ref nodes);
                }
            }
        }

        public static IEnumerable<Node> GetDependentPureNodes(Node node)
        {
            HashSet<Node> nodes = new HashSet<Node>();
            
            AddDependentPureNodes(node, ref nodes);

            return nodes;
        }

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
                    if (evalNode.InputDataPins.All(inNode => inNode.IncomingPin == null || !inNode.IncomingPin.Node.IsPure || sortedNodes.Contains(inNode.IncomingPin.Node)))
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
    }
}
