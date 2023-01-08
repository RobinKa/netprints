using NetPrints.Base;
using NetPrints.Core;
using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    public delegate void InputDataPinIncomingPinChangedDelegate(
        NodeInputDataPin pin, NodeOutputDataPin oldPin, NodeOutputDataPin newPin);

    /// <summary>
    /// Input data pin which can be connected to up to one output data pin to receive a value.
    /// </summary>
    [DataContract]
    public class NodeInputDataPin : NodeDataPin, INodeInputPin
    {
        /// <summary>
        /// Called when the node's incoming pin changed.
        /// </summary>
        public event InputDataPinIncomingPinChangedDelegate IncomingPinChanged;

        /// <summary>
        /// Incoming data pin for this pin. Null when not connected.
        /// Can trigger IncomingPinChanged when set.
        /// </summary>
        public NodeOutputDataPin IncomingPin => IncomingPins.Count > 0 ? (NodeOutputDataPin)IncomingPins[0] : null;

        /// <summary>
        /// Whether this pin uses its unconnected value to output a value
        /// when no pin is connected to it.
        /// </summary>
        public bool UsesUnconnectedValue
        {
            get => PinType.Value is TypeSpecifier t && t.IsPrimitive;
        }

        /// <summary>
        /// Unconnected value of this pin when no pin is connected to it.
        /// Setting this for types that don't support unconnected values will throw
        /// an exception.
        /// </summary>
        [DataMember]
        public object UnconnectedValue
        {
            get => unconnectedValue;
            set
            {
                // Check that:
                // this pin uses the unconnected value
                // the value is of the same type or string if enum

                if (value != null && (!UsesUnconnectedValue
                    || (PinType.Value is TypeSpecifier t && (
                        (!t.IsEnum && TypeSpecifier.FromType(value.GetType()) != t)
                        || (t.IsEnum && value.GetType() != typeof(string))))))
                {
                    throw new ArgumentException();
                }

                unconnectedValue = value;
            }
        }

        private object unconnectedValue;

        [DataMember]
        public object ExplicitDefaultValue
        {
            get;
            set;
        }

        [DataMember]
        public bool UsesExplicitDefaultValue
        {
            get;
            set;
        }

        public IObservableCollectionView<INodeOutputPin> IncomingPins => ConnectedPins.ObservableOfType<INodeOutputPin, INodePin>();

        public override NodePinConnectionType ConnectionType => NodePinConnectionType.Single;

        public NodeInputDataPin(Node node, string name, ObservableValue<BaseType> pinType)
            : base(node, name, pinType)
        {
        }
    }
}
