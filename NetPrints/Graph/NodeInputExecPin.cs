using NetPrints.Core;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    /// <summary>
    /// Pin that can be connected to output execution pins to receive execution.
    /// </summary>
    [DataContract]
    public class NodeInputExecPin : NodeExecPin
    {
        /// <summary>
        /// Output execution pins connected to this pin.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<NodeOutputExecPin> IncomingPins { get; private set; } =
            new ObservableRangeCollection<NodeOutputExecPin>();

        public NodeInputExecPin(Node node, string name)
            : base(node, name)
        {
        }
    }
}
