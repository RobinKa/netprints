using NetPrints.Base;
using NetPrints.Core;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Pin that can be connected to output execution pins to receive execution.
    /// </summary>
    [DataContract]
    public class NodeInputExecPin : NodeExecPin, INodeInputPin
    {
        /// <summary>
        /// Output execution pins connected to this pin.
        /// </summary>
        public IObservableCollectionView<NodeOutputExecPin> IncomingExecutionPins => ConnectedPins.ObservableOfType<NodeOutputExecPin, INodePin>();

        public IObservableCollectionView<INodeOutputPin> IncomingPins => ConnectedPins.ObservableOfType<INodeOutputPin, INodePin>();

        public override NodePinConnectionType ConnectionType => NodePinConnectionType.Multiple;

        public NodeInputExecPin(Node node, string name)
            : base(node, name)
        {
        }
    }
}
