using NetPrints.Core;
using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    public delegate void InputDataPinIncomingPinChangedDelegate(
        NodeInputDataPin pin, NodeOutputDataPin oldPin, NodeOutputDataPin newPin);

    [DataContract]
    public class NodeInputDataPin : NodeDataPin
    {
        public event InputDataPinIncomingPinChangedDelegate IncomingPinChanged;

        [DataMember]
        public NodeOutputDataPin IncomingPin
        {
            get => incomingPin;
            set
            {
                if(incomingPin != value)
                {
                    var oldPin = incomingPin;
                    
                    incomingPin = value;

                    IncomingPinChanged?.Invoke(this, oldPin, incomingPin);
                }
            }
        }

        public bool UsesUnconnectedValue
        {
            get => PinType.IsPrimitive;
        }

        private NodeOutputDataPin incomingPin;

        [DataMember]
        public object UnconnectedValue
        {
            get => unconnectedValue;
            set
            {
                if (value != null && (!UsesUnconnectedValue || value.GetType() != PinType))
                    throw new ArgumentException();

                unconnectedValue = value;
            }
        }

        private object unconnectedValue;

        public NodeInputDataPin(Node node, string name, TypeSpecifier pinType)
            : base(node, name, pinType)
        {
            
        }
    }
}
