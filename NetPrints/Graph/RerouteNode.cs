using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public int TypeRerouteCount { get => InputTypePins.Count; }

        private RerouteNode(Method method)
            : base(method)
        {
            
        }

        public static RerouteNode MakeExecution(Method method, int numExecs)
        {
            var node = new RerouteNode(method);

            for (int i = 0; i < numExecs; i++)
            {
                node.AddInputExecPin($"Exec{i}");
                node.AddOutputExecPin($"Exec{i}");
            }

            return node;
        }

        public static RerouteNode MakeData(Method method, IEnumerable<Tuple<BaseType, BaseType>> dataTypes)
        {
            if (dataTypes is null)
            {
                throw new ArgumentException("dataTypes was null in RerouteNode.MakeData.");
            }

            var node = new RerouteNode(method);

            int index = 0;
            foreach (var dataType in dataTypes)
            {
                node.AddInputDataPin($"Data{index}", dataType.Item1);
                node.AddOutputDataPin($"Data{index}", dataType.Item2);
            }

            return node;
        }

        public static RerouteNode MakeType(Method method, int numTypes)
        {
            var node = new RerouteNode(method);

            for (int i = 0; i < numTypes; i++)
            {
                node.AddInputTypePin($"Type{i}");
                node.AddOutputTypePin($"Type{i}", new ObservableValue<BaseType>(null));
            }

            return node;
        }

        protected override void OnInputTypeChanged(object sender, EventArgs eventArgs)
        {
            base.OnInputTypeChanged(sender, eventArgs);
            OutputTypePins[0].InferredType.Value = InputTypePins[0].InferredType?.Value;
        }

        public override string ToString()
        {
            return "Reroute";
        }
    }
}
