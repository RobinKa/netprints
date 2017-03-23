using NetPrints.Graph;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NetPrints.Core
{
    [Flags]
    public enum MethodModifiers
    {
        Private = 0,
        Public = 1,
        Protected = 2,
        Internal = 4,
        Sealed = 8,
        Abstract = 16,
        Static = 32,
        Virtual = 64,
        Override = 128,
    }

    [DataContract]
    public class Method
    {
        [DataMember]
        public EntryNode EntryNode
        {
            get;
            private set;
        }

        [DataMember]
        public ReturnNode ReturnNode
        {
            get;
            private set;
        }

        [DataMember]
        public string Name
        {
            get;
            set;
        }

        [DataMember]
        public ObservableRangeCollection<Node> Nodes
        {
            get;
            private set;
        } = new ObservableRangeCollection<Node>();

        [DataMember]
        public ObservableRangeCollection<BaseType> ArgumentTypes
        {
            get;
            private set;
        } = new ObservableRangeCollection<BaseType>();

        [DataMember]
        public ObservableRangeCollection<BaseType> ReturnTypes
        {
            get;
            private set;
        } = new ObservableRangeCollection<BaseType>();

        [DataMember]
        public MethodModifiers Modifiers
        {
            get;
            set;
        } = MethodModifiers.Private;

        [DataMember]
        public Class Class
        {
            get;
            set;
        }

        [DataMember]
        public IList<GenericType> DeclaredGenericArguments
        {
            get;
            private set;
        } = new List<GenericType>();

        public Method(string name)
        {
            Name = name;
            EntryNode = new EntryNode(this);
            ReturnNode = new ReturnNode(this);
        }

        [OnDeserialized]
        private void OnDeserializing(StreamingContext c)
        {
            EntryNode.SetupArgumentTypesChangedEvent();
            ReturnNode.SetupReturnTypesChangedEvent();
        }
    }
}
