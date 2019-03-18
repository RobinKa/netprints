using NetPrints.Core;
using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    public delegate void InputTypePinIncomingPinChangedDelegate(
        NodeInputTypePin pin, NodeOutputTypePin oldPin, NodeOutputTypePin newPin);

    /// <summary>
    /// Pin which can receive types.
    /// </summary>
    [DataContract]
    public class NodeInputTypePin : NodeTypePin
    {
        /// <summary>
        /// Called when the node's incoming pin changed.
        /// </summary>
        public event InputTypePinIncomingPinChangedDelegate IncomingPinChanged;

        /// <summary>
        /// Incoming type pin for this pin. Null when not connected.
        /// Can trigger IncomingPinChanged when set.
        /// </summary>
        [DataMember]
        public NodeOutputTypePin IncomingPin
        {
            get => incomingPin;
            set
            {
                if (incomingPin != value)
                {
                    var oldPin = incomingPin;

                    incomingPin = value;

                    IncomingPinChanged?.Invoke(this, oldPin, incomingPin);
                }
            }
        }

        public override BaseType InferredType
        {
            get => IncomingPin?.InferredType;
        }

        private NodeOutputTypePin incomingPin;

        public NodeInputTypePin(Node node, string name)
            : base(node, name)
        {
        }
    }
}
