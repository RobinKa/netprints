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
        Private = 0,
        Public = 1,
        Protected = 2,
        Internal = 4,
        Sealed = 8,
        Abstract = 16,
        Static = 32,
    }

    /// <summary>
    /// Visibility for methods, properties and other members.
    /// </summary>
    [Flags]
    public enum MemberVisibility
    {
        Private = 1,
        Public = 2,
        Protected = 4,
        Internal = 8,

        Any = Private | Public | Protected | Internal,
        ProtectedOrPublic = Protected | Public,
    }

    /// <summary>
    /// Class type. Contains methods, attributes and other common things usually associated
    /// with classes.
    /// </summary>
    [DataContract]
    public class Class
    {
        /// <summary>
        /// Attributes this class has.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<Variable> Attributes { get; set; } = new ObservableRangeCollection<Variable>();

        /// <summary>
        /// Methods this class has.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<Method> Methods { get; set; } = new ObservableRangeCollection<Method>();

        /// <summary>
        /// Base / super type of this class. The ultimate base type of all classes is System.Object.
        /// </summary>
        [DataMember]
        public TypeSpecifier SuperType { get; set; } = TypeSpecifier.FromType<object>();

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
        public ClassModifiers Modifiers { get; set; } = ClassModifiers.Internal;

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

        public Class()
        {

        }
    }
}
