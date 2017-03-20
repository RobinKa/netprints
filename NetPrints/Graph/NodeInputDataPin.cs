using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    [DataContract]
    public class NodeInputDataPin : NodeDataPin
    {
        [DataMember]
        public NodeOutputDataPin IncomingPin
        {
            get;
            set;
        }

        public bool UsesUnconnectedValue
        {
            get => PinType.IsPrimitive || PinType == typeof(string);
        }

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

        public NodeInputDataPin(Node node, string name, Type pinType)
            : base(node, name, pinType)
        {
            
        }
    }
}
