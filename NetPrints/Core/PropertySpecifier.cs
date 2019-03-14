using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Core
{
    /// <summary>
    /// Specifier describing a property of a class.
    /// </summary>
    [Serializable]
    [DataContract]
    public class PropertySpecifier
    {
        /// <summary>
        /// Name of the property without any prefixes.
        /// </summary>
        [DataMember]
        public string Name
        {
            get;
            private set;
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
            private set;
        }

        /// <summary>
        /// Whether this property has a public getter.
        /// </summary>
        [DataMember]
        public bool HasPublicGetter
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether this property has a public setter.
        /// </summary>
        [DataMember]
        public bool HasPublicSetter
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a PropertySpecifier.
        /// </summary>
        /// <param name="name">Name of the property.</param>
        /// <param name="type">Specifier for the type of this property.</param>
        /// <param name="hasPublicGetter">Whether the property has a public getter.</param>
        /// <param name="hasPublicSetter">Whether the property has a public setter.</param>
        /// <param name="declaringType">Specifier for the type the property is contained in.</param>
        public PropertySpecifier(string name, TypeSpecifier type, bool hasPublicGetter, 
            bool hasPublicSetter, TypeSpecifier declaringType)
        {
            Name = name;
            Type = type;
            HasPublicGetter = hasPublicGetter;
            HasPublicSetter = hasPublicSetter;
            DeclaringType = declaringType;
        }
    }
}
