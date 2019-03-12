using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Represents a node which returns from a method.
    /// </summary>
    [DataContract]
    public class ReturnNode : Node
    {
        /// <summary>
        /// Execution pin that returns from the method when executed.
        /// </summary>
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

        /// <summary>
        /// Updates the input data pins of this node to match the given list of types.
        /// Keeps old pins connected to what they were connected to if they still exist.
        /// </summary>
        /// <param name="returnTypes">List of specifiers for types this node should have as data inputs.</param>
        public void SetReturnTypes(IEnumerable<BaseType> returnTypes)
        {
            Dictionary<int, NodeOutputDataPin> oldConnections = new Dictionary<int, NodeOutputDataPin>();

            foreach (NodeInputDataPin pin in InputDataPins)
            {
                // Remember pins with same type as before
                int i = InputDataPins.IndexOf(pin);
                if (i < returnTypes.Count() && pin.PinType == returnTypes.ElementAt(i)
                    && pin.IncomingPin != null)
                {
                    oldConnections.Add(i, pin.IncomingPin);
                }

                GraphUtil.DisconnectInputDataPin(pin);
            }

            InputDataPins.Clear();

            foreach (TypeSpecifier returnType in returnTypes)
            {
                AddInputDataPin(returnType.ShortName, returnType);
            }

            foreach (var oldConn in oldConnections)
            {
                GraphUtil.ConnectDataPins(oldConn.Value, InputDataPins[oldConn.Key]);
            }
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
