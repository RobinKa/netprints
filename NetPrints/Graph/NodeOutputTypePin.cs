using NetPrints.Base;
using NetPrints.Core;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Pin which outputs a type. Can be connected to input type pins.
    /// </summary>
    [DataContract]
    public class NodeOutputTypePin : NodeTypePin, INodeOutputPin
    {
        /// <summary>
        /// Connected input data pins.
        /// </summary>
        public IObservableCollectionView<NodeInputTypePin> OutgoingTypePins => ConnectedPins.ObservableOfType<NodeInputTypePin, INodePin>();

        [DataMember]
        public override ObservableValue<BaseType> InferredType { get; }

        public IObservableCollectionView<INodeInputPin> OutgoingPins => ConnectedPins.ObservableOfType<INodeInputPin, INodePin>();

        public override NodePinConnectionType ConnectionType => NodePinConnectionType.Multiple;

        public NodeOutputTypePin(Node node, string name, ObservableValue<BaseType> outputType)
            : base(node, name)
        {
            InferredType = outputType;
        }

        public override string ToString()
        {
            return InferredType.Value?.ShortName ?? "None";
        }
    }
}
