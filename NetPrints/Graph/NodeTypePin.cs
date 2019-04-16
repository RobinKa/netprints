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
        public abstract ObservableValue<BaseType> InferredType
        {
            get;
        }

        protected NodeTypePin(Node node, string name)
            : base(node, name)
        {
        }
    }
}
