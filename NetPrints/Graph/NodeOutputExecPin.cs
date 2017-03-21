using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    public delegate void OutputExecPinOutgoingPinChangedDelegate(
        NodeOutputExecPin pin, NodeInputExecPin oldPin, NodeInputExecPin newPin);

    [DataContract]
    public class NodeOutputExecPin : NodeExecPin
    {
        public event OutputExecPinOutgoingPinChangedDelegate OutgoingPinChanged;

        [DataMember]
        public NodeInputExecPin OutgoingPin
        {
            get => outgoingPin;
            set
            {
                if(outgoingPin != value)
                {
                    var oldPin = outgoingPin;
                    
                    outgoingPin = value;

                    OutgoingPinChanged?.Invoke(this, oldPin, outgoingPin);
                }
            }
        }

        private NodeInputExecPin outgoingPin;

        public NodeOutputExecPin(Node node, string name)
            : base(node, name)
        {
            
        }
    }
}
