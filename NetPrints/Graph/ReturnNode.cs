using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    public class ReturnNode : Node
    {
        public NodeInputExecPin ReturnPin
        {
            get { return InputExecPins[0]; }
        }
        
        public ReturnNode(Method method)
            : base(method)
        {
            AddInputExecPin("Exec");

            SetReturnTypes(method.ReturnTypes);
            SetupReturnTypesChangedEvent();
        }

        public void SetReturnTypes(IEnumerable<TypeSpecifier> returnTypes)
        {
            List<NodeOutputDataPin> oldConnections = new List<NodeOutputDataPin>();

            foreach (NodeInputDataPin pin in InputDataPins)
            {
                oldConnections.Add(pin.IncomingPin);
                GraphUtil.DisconnectInputDataPin(pin);
            }

            InputDataPins.Clear();

            foreach (TypeSpecifier returnType in returnTypes)
            {
                AddInputDataPin(returnType.ShortName, returnType);
            }

            /* TODO: Replace IsSubclassOf
            // Try to reconnect old pins
            for (int i = 0; i < Math.Min(oldConnections.Count, InputDataPins.Count); i++)
            {
                Type returnType = InputDataPins[i].PinType;
                
                var connPin = oldConnections[i];

                if(connPin != null)
                {
                    if (returnType == connPin.PinType || connPin.PinType.IsSubclassOf(returnType))
                    {
                        GraphUtil.ConnectDataPins(connPin, InputDataPins[i]);
                    }
                }
            }*/
        }

        // Called in constructor or after method has been deserialized
        public void SetupReturnTypesChangedEvent()
        {
            Method.ReturnTypes.CollectionChanged += OnReturnTypesChanged;
        }

        private void OnReturnTypesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SetReturnTypes(Method.ReturnTypes);
        }
    }
}
