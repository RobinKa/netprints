using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Graph
{
    [DataContract]
    public class TypeNode
    {
        [DataMember]
        public ObservableRangeCollection<NodeInputTypePin> InputTypePins { get; private set; } = new ObservableRangeCollection<NodeInputTypePin>();

        public TypeSpecifier Type
        {
            get;
            private set;
        }

        public TypeNode(TypeGraph typeGraph, TypeSpecifier type)
        {
            foreach(TypeSpecifier genArg in type.GenericArguments)
            {
                AddInputTypePin(null);
            }
        }

        protected void AddInputTypePin(NodeTypeConstraints constraints)
        {
            InputTypePins.Add(new NodeInputTypePin(this, constraints));
        }
    }
}
