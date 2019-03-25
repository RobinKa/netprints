using NetPrints.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Core
{
    /// <summary>
    /// Modifiers a method can have. Can be combined.
    /// </summary>
    [Flags]
    public enum MethodModifiers
    {
        None = 0,
        Sealed = 8,
        Abstract = 16,
        Static = 32,
        Virtual = 64,
        Override = 128,

        // DEPRECATED
        // Moved to MethodVisibility
        Private = 0,
        Public = 1,
        Protected = 2,
        Internal = 4,
    }

    /// <summary>
    /// Method type. Contains common things usually associated with methods such as its arguments and its name.
    /// </summary>
    [DataContract]
    public partial class Method
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
        public IEnumerable<ReturnNode> ReturnNodes
        {
            get => Nodes.OfType<ReturnNode>();
        }

        /// <summary>
        /// Main return node that determines the return types of all other return nodes.
        /// </summary>
        public ReturnNode MainReturnNode
        {
            get => Nodes?.OfType<ReturnNode>()?.FirstOrDefault();
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
        public IEnumerable<BaseType> ArgumentTypes
        {
            get => EntryNode != null ? EntryNode.InputTypePins.Select(pin => pin.InferredType?.Value ?? TypeSpecifier.FromType<object>()).ToList() : new List<BaseType>();
        }

        /// <summary>
        /// Generic type arguments of the method.
        /// </summary>
        public IEnumerable<GenericType> GenericArgumentTypes
        {
            get => EntryNode != null ? EntryNode.OutputTypePins.Select(pin => pin.InferredType.Value).Cast<GenericType>().ToList() : new List<GenericType>();
        }

        /// <summary>
        /// Ordered return types this method returns.
        /// </summary>
        public IEnumerable<BaseType> ReturnTypes
        {
            get => MainReturnNode?.InputTypePins?.Select(pin => pin.InferredType?.Value ?? TypeSpecifier.FromType<object>())?.ToList() ?? new List<BaseType>();
        }

        /// <summary>
        /// Modifiers this method has.
        /// </summary>
        [DataMember]
        public MethodModifiers Modifiers
        {
            get;
            set;
        } = MethodModifiers.None;

        /// <summary>
        /// Visibility of this method.
        /// </summary>
        [DataMember]
        public MemberVisibility Visibility
        {
            get;
            set;
        } = MemberVisibility.Private;

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
        /// Creates a method given its name.
        /// </summary>
        /// <param name="name">Name for the method.</param>
        public Method(string name)
        {
            Name = name;
            EntryNode = new EntryNode(this);
            new ReturnNode(this);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            foreach (var node in Nodes)
            {
                node.OnMethodDeserialized();
            }
        }
    }
}
