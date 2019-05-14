using NetPrints.Base;
using NetPrints.Core;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Pin which outputs a value. Can be connected to input data pins.
    /// </summary>
    [DataContract]
    public class NodeOutputDataPin : NodeDataPin, INodeOutputPin
    {
        /// <summary>
        /// Connected input data pins.
        /// </summary>
        public IObservableCollectionView<NodeInputDataPin> OutgoingDataPins => ConnectedPins.ObservableOfType<NodeInputDataPin, INodePin>();

        public IObservableCollectionView<INodeInputPin> OutgoingPins => ConnectedPins.ObservableOfType<INodeInputPin, INodePin>();

        public override NodePinConnectionType ConnectionType => NodePinConnectionType.Multiple;

        public NodeOutputDataPin(Node node, string name, ObservableValue<BaseType> pinType)
            : base(node, name, pinType)
        {
        }
    }
}
