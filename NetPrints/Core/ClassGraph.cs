using NetPrints.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Core
{
    /// <summary>
    /// Modifiers a class can have. Can be combined.
    /// </summary>
    [Flags]
    public enum ClassModifiers
    {
        None = 0,
        Sealed = 8,
        Abstract = 16,
        Static = 32,
        Partial = 64,

        // Deprecated
        [Obsolete]
        Private = 0,
        [Obsolete]
        Public = 1,
        [Obsolete]
        Protected = 2,
        [Obsolete]
        Internal = 4,
    }

    /// <summary>
    /// Visibility for methods, properties and other members.
    /// </summary>
    [Flags]
    public enum MemberVisibility
    {
        Invalid = 0,
        Private = 1,
        Public = 2,
        Protected = 4,
        Internal = 8,

        Any = Private | Public | Protected | Internal,
        ProtectedOrPublic = Protected | Public,
    }

    /// <summary>
    /// Class graph type. Contains methods, attributes and other common things usually associated
    /// with classes.
    /// </summary>
    [DataContract]
    public partial class ClassGraph : NodeGraph
    {
        /// <summary>
        /// Return node of this class that receives the metadata for it.
        /// </summary>
        public ClassReturnNode ReturnNode
        {
            get => Nodes.OfType<ClassReturnNode>().Single();
        }

        /// <summary>
        /// Properties of this class.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<Variable> Variables { get; set; } = new ObservableRangeCollection<Variable>();

        /// <summary>
        /// Methods of this class.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<MethodGraph> Methods { get; set; } = new ObservableRangeCollection<MethodGraph>();

        /// <summary>
        /// Constructors of this class.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<ConstructorGraph> Constructors { get; set; } = new ObservableRangeCollection<ConstructorGraph>();

        /// <summary>
        /// Base / super type of this class. The ultimate base type of all classes is System.Object.
        /// </summary>
        public TypeSpecifier SuperType
        {
            get => (TypeSpecifier)ReturnNode.SuperTypePin.InferredType?.Value ?? TypeSpecifier.FromType<object>();
        }

        /// <summary>
        /// Type this class inherits from and interfaces this class implements.
        /// </summary>
        public IEnumerable<TypeSpecifier> AllBaseTypes
        {
            get => new[] { SuperType }.Concat(ReturnNode.InterfacePins.Select(pin => (TypeSpecifier)pin.InferredType?.Value ?? TypeSpecifier.FromType<object>()));
        }

        /// <summary>
        /// Namespace this class is in.
        /// </summary>
        [DataMember]
        public string Namespace { get; set; }

        /// <summary>
        /// Name of the class without namespace.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Name of the class with namespace if any.
        /// </summary>
        public string FullName
        {
            get => string.IsNullOrWhiteSpace(Namespace) ? Name : $"{Namespace}.{Name}";
        }

        /// <summary>
        /// Modifiers this class has.
        /// </summary>
        [DataMember]
        public ClassModifiers Modifiers { get; set; }

        /// <summary>
        /// Visibility of this class.
        /// </summary>
        [DataMember]
        public MemberVisibility Visibility { get; set; } = MemberVisibility.Internal;

        /// <summary>
        /// Generic arguments this class takes.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<GenericType> DeclaredGenericArguments { get; set; } = new ObservableRangeCollection<GenericType>();

        /// <summary>
        /// TypeSpecifier describing this class.
        /// </summary>
        public TypeSpecifier Type
        {
            get => new TypeSpecifier(FullName, SuperType.IsEnum, SuperType.IsInterface,
                DeclaredGenericArguments.Cast<BaseType>().ToList());
        }

        public ClassGraph()
        {
            _ = new ClassReturnNode(this);
        }
    }
}
