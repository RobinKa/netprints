using System.Runtime.Serialization;

namespace NetPrints.Core
{
    [DataContract]
    public class VariableSpecifier
    {
        /// <summary>
        /// Name of the property without any prefixes.
        /// </summary>
        [DataMember]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Specifier for the type this property is contained in.
        /// </summary>
        [DataMember]
        public TypeSpecifier DeclaringType
        {
            get;
            private set;
        }

        /// <summary>
        /// Specifier for the type of the property.
        /// </summary>
        [DataMember]
        public TypeSpecifier Type
        {
            get;
            set;
        }

        /// <summary>
        /// Whether this property has a public getter.
        /// </summary>
        [DataMember]
        public MemberVisibility GetterVisibility
        {
            get;
            set;
        }

        /// <summary>
        /// Whether this property has a public setter.
        /// </summary>
        [DataMember]
        public MemberVisibility SetterVisibility
        {
            get;
            set;
        }

        /// <summary>
        /// Visibility of this property.
        /// </summary>
        [DataMember]
        public MemberVisibility Visibility
        {
            get;
            set;
        } = MemberVisibility.Private;

        /// <summary>
        /// Modifiers of this variable.
        /// </summary>
        [DataMember]
        public VariableModifiers Modifiers
        {
            get;
            set;
        }

        public VariableSpecifier(string name, TypeSpecifier type, MemberVisibility getterVisibility, MemberVisibility setterVisibility,
            TypeSpecifier declaringType, VariableModifiers modifiers)
        {
            Name = name;
            Type = type;
            GetterVisibility = getterVisibility;
            SetterVisibility = setterVisibility;
            DeclaringType = declaringType;
            Modifiers = modifiers;
        }
    }
}
