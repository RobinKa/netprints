using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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

        public void SetArgumentTypes(IEnumerable<Type> parameterTypes)
        {
            List<List<NodeInputDataPin>> oldConnections = new List<List<NodeInputDataPin>>();

            foreach (NodeOutputDataPin pin in OutputDataPins)
            {
                oldConnections.Add(new List<NodeInputDataPin>(pin.OutgoingPins));
                GraphUtil.DisconnectOutputDataPin(pin);
            }

            OutputDataPins.Clear();

            foreach(Type paramType in parameterTypes)
            {
                AddOutputDataPin(paramType.Name, paramType);
            }

            // Try to reconnect old pins
            for(int i = 0; i < Math.Min(oldConnections.Count, OutputDataPins.Count); i++)
            {
                Type paramType = OutputDataPins[i].PinType;
                
                var oldConn = oldConnections[i];

                foreach (var connPin in oldConn)
                {
                    if (paramType == connPin.PinType || paramType.IsSubclassOf(connPin.PinType))
                    {
                        GraphUtil.ConnectDataPins(OutputDataPins[i], connPin);
                    }
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
