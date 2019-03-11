using NetPrints.Core;
using System;
using System.Runtime.Serialization;

namespace NetPrints.Graph
{
    public class NodeTypeConstraints
    {
        // TODO: Add constraints
    }

    public delegate void TypeNodeIncomingNodeChanged(
        NodeInputTypePin pin, TypeNode oldNode, TypeNode newNode);

    /// <summary>
    /// Pin which can receive types.
    /// </summary>
    [DataContract]
    public class NodeInputTypePin
    {
        /// <summary>
        /// Constraints for the type.
        /// </summary>
        [DataMember]
        public NodeTypeConstraints Constraints
        {
            get;
            private set;
        }

        public event TypeNodeIncomingNodeChanged IncomingNodeChanged;

        [DataMember]
        public TypeNode IncomingNode
        {
            get => incomingNode;
            set
            {
                if (incomingNode != value)
                {
                    var oldNode = incomingNode;

                    incomingNode = value;

                    IncomingNodeChanged?.Invoke(this, oldNode, incomingNode);
                }
            }
        }

        private TypeNode incomingNode;

        [DataMember]
        public TypeNode Node
        {
            get;
            private set;
        }

        public NodeInputTypePin(TypeNode node, NodeTypeConstraints constraints)
        {
            Node = node;
            Constraints = constraints;
        }
    }
}
