using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<NodeInputTypePin> InterfacePins
        {
            get => InputTypePins.Skip(1);
        }

        public ClassReturnNode(ClassGraph graph)
            : base(graph)
        {
            AddInputTypePin("BaseType");
        }

        public void AddInterfacePin()
        {
            AddInputTypePin($"Interface{InputTypePins.Count}");
        }

        public void RemoveInterfacePin()
        {
            var interfacePin = InterfacePins.LastOrDefault();

            if (interfacePin != null)
            {
                GraphUtil.DisconnectInputTypePin(interfacePin);
                InputTypePins.Remove(interfacePin);
            }
        }
    }
}
