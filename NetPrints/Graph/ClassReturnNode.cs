using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NetPrints.Graph
{
    [DataContract]
    public class ClassReturnNode : Node
    {
        public NodeInputTypePin SuperTypePin
        {
            get => InputTypePins[0];
        }

        public ClassReturnNode(ClassGraph graph)
            : base(graph)
        {
            AddInputTypePin("BaseType");
        }
    }
}
