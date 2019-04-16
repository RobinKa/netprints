using NetPrints.Core;
using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Pin which outputs a value. Can be connected to input data pins.
    /// </summary>
    [DataContract]
    public class NodeOutputDataPin : NodeDataPin
    {
        /// <summary>
        /// Connected input data pins.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<NodeInputDataPin> OutgoingPins { get; private set; }
            = new ObservableRangeCollection<NodeInputDataPin>();

        public NodeOutputDataPin(Node node, string name, ObservableValue<BaseType> pinType)
            : base(node, name, pinType)
        {
        }
    }
}
