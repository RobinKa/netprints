using NetPrints.Base;
using NetPrints.Core;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    public delegate void InputTypePinIncomingPinChangedDelegate(
        NodeInputTypePin pin, NodeOutputTypePin oldPin, NodeOutputTypePin newPin);

    /// <summary>
    /// Pin which can receive types.
    /// </summary>
    [DataContract]
    public class NodeInputTypePin : NodeTypePin, INodeInputPin
    {
        /// <summary>
        /// Called when the node's incoming pin changed.
        /// </summary>
        public event InputTypePinIncomingPinChangedDelegate IncomingPinChanged;

        /// <summary>
        /// Incoming type pin for this pin. Null when not connected.
        /// Can trigger IncomingPinChanged when set.
        /// </summary>
        public NodeOutputTypePin IncomingPin => IncomingPins.Count > 0 ? (NodeOutputTypePin)IncomingPins[0] : null;

        public override ObservableValue<BaseType> InferredType
        {
            get => IncomingPin?.InferredType;
        }

        public IObservableCollectionView<INodeOutputPin> IncomingPins => ConnectedPins.ObservableOfType<INodeOutputPin, INodePin>();

        public override NodePinConnectionType ConnectionType => NodePinConnectionType.Single;

        public NodeInputTypePin(Node node, string name)
            : base(node, name)
        {
        }
    }
}
