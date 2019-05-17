using NetPrints.Base;
using NetPrints.Core;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    public delegate void OutputExecPinOutgoingPinChangedDelegate(
        NodeOutputExecPin pin, NodeInputExecPin oldPin, NodeInputExecPin newPin);

    /// <summary>
    /// Pin which can be connected to an input execution pin to pass along execution.
    /// </summary>
    [DataContract]
    public class NodeOutputExecPin : NodeExecPin, INodeOutputPin
    {
        /// <summary>
        /// Called when the connected outgoing pin changed.
        /// </summary>
        public event OutputExecPinOutgoingPinChangedDelegate OutgoingPinChanged;

        /// <summary>
        /// Connected input execution pin. Null if not connected.
        /// Can trigger OutgoingPinChanged when set.
        /// </summary>
        
        public NodeInputExecPin OutgoingExecPin => OutgoingPins.Count > 0 ? (NodeInputExecPin)OutgoingPins[0] : null;

        public IObservableCollectionView<INodeInputPin> OutgoingPins => ConnectedPins.ObservableOfType<INodeInputPin, INodePin>();

        public override NodePinConnectionType ConnectionType => NodePinConnectionType.Single;

        public NodeOutputExecPin(Node node, string name)
            : base(node, name)
        {
        }
    }
}
