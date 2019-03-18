using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NetPrints.Graph
{
    /// <summary>
    /// Abstract class for type pins.
    /// </summary>
    [DataContract]
    public abstract class NodeTypePin : NodePin
    {
        public abstract BaseType InferredType
        {
            get;
        }

        public NodeTypePin(Node node, string name)
            : base(node, name)
        {
        }
    }
}
