using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Linq;

namespace NetPrints.Graph
{
    [DataContract]
    public class EntryNode : Node
    {
        public NodeOutputExecPin InitialExecutionPin
        {
            get { return OutputExecPins[0]; }
        }

        public EntryNode(Method method)
            : base(method)
        {
            AddOutputExecPin("Exec");

            SetArgumentTypes(Method.ArgumentTypes);
            SetupArgumentTypesChangedEvent();
        }

        public void SetArgumentTypes(IEnumerable<BaseType> parameterTypes)
        {
            Dictionary<int, IEnumerable<NodeInputDataPin>> oldConnections =
                new Dictionary<int, IEnumerable<NodeInputDataPin>>();

            // Disconnect all current connections and remember them
            foreach (NodeOutputDataPin pin in OutputDataPins)
            {
                // Remember pins with same type as before
                int i = OutputDataPins.IndexOf(pin);
                if (i < parameterTypes.Count() && pin.PinType == parameterTypes.ElementAt(i))
                {
                    oldConnections.Add(i, new List<NodeInputDataPin>(pin.OutgoingPins));
                }

                GraphUtil.DisconnectOutputDataPin(pin);
            }

            OutputDataPins.Clear();

            foreach (BaseType paramType in parameterTypes)
            {
                AddOutputDataPin(paramType.ShortName, paramType);
            }

            foreach (var oldConn in oldConnections)
            {
                foreach (NodeInputDataPin toPin in oldConn.Value)
                {
                    GraphUtil.ConnectDataPins(OutputDataPins[oldConn.Key], toPin);
                }
            }
        }

        // Called in constructor or after method has been deserialized
        public void SetupArgumentTypesChangedEvent()
        {
            Method.ArgumentTypes.CollectionChanged += OnArgumentTypesChanged;
        }

        private void OnArgumentTypesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SetArgumentTypes(Method.ArgumentTypes);
        }

        public override string ToString()
        {
            return $"{Method.Name} Entry";
        }
    }
}
