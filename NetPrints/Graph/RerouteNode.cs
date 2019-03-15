using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Node representing a reroute node. Does nothing by itself.
    /// Used for layouting in the editor.
    /// </summary>
    [DataContract]
    public class RerouteNode : Node
    {
        public int ExecRerouteCount { get => InputExecPins.Count; }
        public int DataRerouteCount { get => InputDataPins.Count; }

        public RerouteNode(Method method, int numExecs, IEnumerable<Tuple<BaseType, BaseType>> dataTypes)
            : base(method)
        {
            for (int i = 0; i < numExecs; i++)
            {
                AddInputExecPin($"Exec{i}");
                AddOutputExecPin($"Exec{i}");
            }

            if (dataTypes != null)
            {
                int index = 0;
                foreach (var dataType in dataTypes)
                {
                    AddInputDataPin($"Data{index}", dataType.Item1);
                    AddOutputDataPin($"Data{index}", dataType.Item2);
                }
            }
        }

        public override string ToString()
        {
            return "Reroute";
        }
    }
}
