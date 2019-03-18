using NetPrints.Core;
using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Pin which outputs a type. Can be connected to input type pins.
    /// </summary>
    [DataContract]
    public class NodeOutputTypePin : NodeTypePin
    {
        /// <summary>
        /// Connected input data pins.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<NodeInputTypePin> OutgoingPins { get; private set; } 
            = new ObservableRangeCollection<NodeInputTypePin>();

        public override BaseType InferredType
        {
            get => getOutputType();
        }

        private Func<BaseType> getOutputType;

        public NodeOutputTypePin(Node node, string name, Func<BaseType> getOutputTypeFunc)
            : base(node, name)
        {
            getOutputType = getOutputTypeFunc;
        }
    }
}
