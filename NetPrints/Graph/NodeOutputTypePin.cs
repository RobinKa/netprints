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

        public override ObservableValue<BaseType> InferredType
        {
            get => outputType;
        }

        [DataMember]
        private ObservableValue<BaseType> outputType;

        public NodeOutputTypePin(Node node, string name, ObservableValue<BaseType> outputType)
            : base(node, name)
        {
            this.outputType = outputType;
        }

        public override string ToString()
        {
            return outputType.Value?.ShortName ?? "None";
        }
    }
}
