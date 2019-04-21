using NetPrints.Core;
using System;
using System.Collections.Generic;
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

        public ReturnNode(MethodGraph graph)
            : base(graph)
        {
            AddInputExecPin("Exec");

            SetupSecondaryNodeEvents();
        }

        /// <summary>
        /// Sets the data pin types to the same as the main nodes' data pin types.
        /// </summary>
        private void ReplicateMainNodeInputTypes()
        {
            if (this == MethodGraph.MainReturnNode)
            {
                return;
            }

            // Get new return types
            NodeInputDataPin[] mainInputPins = MethodGraph.MainReturnNode.InputDataPins.ToArray();

            var oldConnections = new Dictionary<int, NodeOutputDataPin>();

            // Remember pins with same type as before
            foreach (NodeInputDataPin pin in InputDataPins)
            {
                int i = InputDataPins.IndexOf(pin);
                if (i < mainInputPins.Length && pin.IncomingPin != null)
                {
                    oldConnections.Add(i, pin.IncomingPin);
                }

                GraphUtil.DisconnectInputDataPin(pin);
            }

            InputDataPins.Clear();

            foreach (NodeInputDataPin mainInputPin in mainInputPins)
            {
                AddInputDataPin(mainInputPin.Name, mainInputPin.PinType.Value);
            }

            // Restore old connections
            foreach (var oldConn in oldConnections)
            {
                GraphUtil.ConnectDataPins(oldConn.Value, InputDataPins[oldConn.Key]);
            }
        }

        /// <summary>
        /// Sets the data pin types to the input type pin types.
        /// </summary>
        private void UpdateMainNodeInputTypes()
        {
            if (this != MethodGraph.MainReturnNode)
            {
                return;
            }

            for (int i = 0; i < InputTypePins.Count; i++)
            {
                InputDataPins[i].PinType.Value = InputTypePins[i].InferredType?.Value ?? TypeSpecifier.FromType<object>();
            }
        }

        protected override void OnInputTypeChanged(object sender, EventArgs eventArgs)
        {
            base.OnInputTypeChanged(sender, eventArgs);
            UpdateMainNodeInputTypes();
        }

        public void AddReturnType()
        {
            if (this != MethodGraph.MainReturnNode)
            {
                throw new InvalidOperationException("Can only add return types on the main return node.");
            }

            int returnIndex = InputDataPins.Count;

            AddInputDataPin($"Output{returnIndex}", new ObservableValue<BaseType>(TypeSpecifier.FromType<object>()));
            AddInputTypePin($"Output{returnIndex}Type");
        }

        public void RemoveReturnType()
        {
            if (this != MethodGraph.MainReturnNode)
            {
                throw new InvalidOperationException("Can only remove return types on the main return node.");
            }

            if (InputDataPins.Count > 0)
            {
                NodeInputDataPin idpToRemove = InputDataPins.Last();
                NodeInputTypePin itpToRemove = InputTypePins.Last();

                GraphUtil.DisconnectInputDataPin(idpToRemove);
                GraphUtil.DisconnectInputTypePin(itpToRemove);

                InputDataPins.Remove(idpToRemove);
                InputTypePins.Remove(itpToRemove);
            }
        }

        private void SetupSecondaryNodeEvents()
        {
            if (MethodGraph.MainReturnNode != null)
            {
                if (this == MethodGraph.MainReturnNode)
                {
                    UpdateMainNodeInputTypes();
                }
                else
                {
                    MethodGraph.MainReturnNode.InputDataPins.CollectionChanged += (sender, e) => ReplicateMainNodeInputTypes();
                    MethodGraph.MainReturnNode.InputTypeChanged += (sender, e) => ReplicateMainNodeInputTypes();
                    ReplicateMainNodeInputTypes();
                }
            }
        }

        public override string ToString()
        {
            return "Return";
        }
    }
}
