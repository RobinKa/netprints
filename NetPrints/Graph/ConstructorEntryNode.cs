using NetPrints.Core;
using System;
using System.Runtime.Serialization;
using System.Linq;

namespace NetPrints.Graph
{
    /// <summary>
    /// Node representing the initial execution node of a constructor.
    /// </summary>
    [DataContract]
    public class ConstructorEntryNode : ExecutionEntryNode
    {
        public ConstructorGraph ConstructorGraph
        {
            get => (ConstructorGraph)Graph;
        }

        public ConstructorEntryNode(ConstructorGraph constructor)
            : base(constructor)
        {
            AddOutputExecPin("Exec");

            // TODO: Add output data and type pins for constructor graph
        }

        public override string ToString()
        {
            return $"{ConstructorGraph.Class.Name} Constructor Entry";
        }
    }
}
