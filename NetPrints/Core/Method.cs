using NetPrints.Graph;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NetPrints.Core
{
    /// <summary>
    /// Modifiers a method can have. Can be combined.
    /// </summary>
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

    /// <summary>
    /// Method type. Contains common things usually associated with methods such as its arguments and its name.
    /// </summary>
    [DataContract]
    public class Method
    {
        /// <summary>
        /// Entry node of this method where execution starts.
        /// </summary>
        [DataMember]
        public EntryNode EntryNode
        {
            get;
            private set;
        }

        /// <summary>
        /// Return node of this method that when executed will return from the method.
        /// </summary>
        [DataMember]
        public ReturnNode ReturnNode
        {
            get;
            private set;
        }

        /// <summary>
        /// Name of the method without any prefixes.
        /// </summary>
        [DataMember]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Collection of nodes in this method.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<Node> Nodes
        {
            get;
            private set;
        } = new ObservableRangeCollection<Node>();

        /// <summary>
        /// Ordered argument types this method takes.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<BaseType> ArgumentTypes
        {
            get;
            private set;
        } = new ObservableRangeCollection<BaseType>();

        /// <summary>
        /// Ordered return types this method returns.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<BaseType> ReturnTypes
        {
            get;
            private set;
        } = new ObservableRangeCollection<BaseType>();

        /// <summary>
        /// Modifiers this method has.
        /// </summary>
        [DataMember]
        public MethodModifiers Modifiers
        {
            get;
            set;
        } = MethodModifiers.Private;

        /// <summary>
        /// Class this method is contained in.
        /// </summary>
        [DataMember]
        public Class Class
        {
            get;
            set;
        }

        /// <summary>
        /// Generic arguments this method takes.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<GenericType> DeclaredGenericArguments
        {
            get;
            private set;
        } = new ObservableRangeCollection<GenericType>();

        /// <summary>
        /// Creates a method given its name.
        /// </summary>
        /// <param name="name">Name for the method.</param>
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
