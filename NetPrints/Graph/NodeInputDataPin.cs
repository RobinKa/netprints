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
            get => PinType is TypeSpecifier t && t.IsPrimitive;
        }

        private NodeOutputDataPin incomingPin;

        [DataMember]
        public object UnconnectedValue
        {
            get => unconnectedValue;
            set
            {
                // Check that:
                // this pin uses the unconnected value
                // the value is of the same type or string if enum

                if (value != null && (!UsesUnconnectedValue || 
                    (PinType is TypeSpecifier t && (
                        (!t.IsEnum && TypeSpecifier.FromType(value.GetType()) != t) ||
                        (t.IsEnum && value.GetType() != typeof(string))))))
                {
                    throw new ArgumentException();
                }

                unconnectedValue = value;
            }
        }

        private object unconnectedValue;

        public NodeInputDataPin(Node node, string name, BaseType pinType)
            : base(node, name, pinType)
        {
            
        }
    }
}
