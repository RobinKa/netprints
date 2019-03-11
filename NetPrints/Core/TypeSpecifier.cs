using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Core
{
    /// <summary>
    /// Specifier describing "real" types (not purely unbound generic).
    /// </summary>
    [DataContract]
    [Serializable]
    public class TypeSpecifier : BaseType
    {
        /// <summary>
        /// Whether this type is an enum.
        /// </summary>
        [DataMember]
        public bool IsEnum
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether this type is an interface.
        /// </summary>
        [DataMember]
        public bool IsInterface
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Generic arguments this type takes.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<BaseType> GenericArguments
        {
            get;
            private set;
        }

        /// <summary>
        /// Short name of the type (ie. name without namespace).
        /// </summary>
        public override string ShortName
        {
            get
            {
                return Name.Split('.').Last();
            }
        }

        /// <summary>
        /// Whether this type is a primitive type (eg. int, bool, float, Enum, ...).
        /// </summary>
        public bool IsPrimitive
        {
            get
            {
                return this == FromType<byte>() || this == FromType<char>() ||
                    this == FromType<short>() || this == FromType<ushort>() ||
                    this == FromType<int>() || this == FromType<uint>() ||
                    this == FromType<long>() || this == FromType<ulong>() ||
                    this == FromType<float>() || this == FromType<double>() ||
                    this == FromType<string>() || this == FromType<bool>() ||
                    IsEnum;
            }
        }
        
        /// <summary>
        /// Creates a TypeSpecifier describing a type.
        /// </summary>
        /// <param name="typeName">Full name of the type including the namespace (ie. Namespace.TypeName).</param>
        /// <param name="isEnum">Whether the type is an enum.</param>
        /// <param name="isInterface">Whether the type is an interface.</param>
        /// <param name="genericArguments">Generic arguments the type takes.</param>
        public TypeSpecifier(string typeName, bool isEnum=false, bool isInterface=false, IList<BaseType> genericArguments=null)
            : base(typeName)
        {
            IsEnum = isEnum;
            IsInterface = IsInterface;

            if(genericArguments == null)
            {
                GenericArguments = new ObservableRangeCollection<BaseType>();
            }
            else
            {
                GenericArguments = new ObservableRangeCollection<BaseType>(genericArguments);
            }
        }

        /// <summary>
        /// Creates a TypeSpecifier for a given type.
        /// </summary>
        /// <typeparam name="T">Type to create a TypeSpecifier for.</typeparam>
        /// <returns>TypeSpecifier for the given type.</returns>
        public static TypeSpecifier FromType<T>()
        {
            return FromType(typeof(T));
        }

        /// <summary>
        /// Creates a TypeSpecifier for a given type.
        /// </summary>
        /// <param name="type">Type to create a TypeSpecifier for.</param>
        /// <returns>TypeSpecifier for the given type.</returns>
        public static TypeSpecifier FromType(Type type)
        {
            return new TypeSpecifier(type.FullName, type.IsSubclassOf(typeof(Enum)), type.IsInterface);
        }
        
        public override bool Equals(object obj)
        {
            if (obj is TypeSpecifier t)
            {
                // Name equal
                // Generic arguments equal
                // IsEnum equal

                if (Name == t.Name && GenericArgumentsEqual(t))
                {
                    if (IsEnum != t.IsEnum)
                        throw new ArgumentException("obj has same type name but IsEnum is different");

                    return true;
                }
            }
            else if(obj is GenericType genType)
            {
                // TODO: Check constraints
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether the generic arguments for this type and a given type match.
        /// </summary>
        /// <param name="t">Specifier for the type to check.</param>
        /// <returns>Whether the generic arguments for the types match.</returns>
        public bool GenericArgumentsEqual(TypeSpecifier t)
        {
            return GenericArguments.SequenceEqual(t.GenericArguments);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            string s = Name;

            if(GenericArguments.Count > 0)
            {
                s += "<" + string.Join(", ", GenericArguments) + ">";
            }

            return s;
        }

        public static implicit operator TypeSpecifier(string typeName)
        {
            return new TypeSpecifier(typeName);
        }

        public static implicit operator string(TypeSpecifier specifier)
        {
            return specifier.Name;
        }

        public static bool operator ==(TypeSpecifier a, TypeSpecifier b)
        {
            if (ReferenceEquals(b, null))
            {
                return ReferenceEquals(a, null);
            }

            return a.Equals(b);
        }

        public static bool operator !=(TypeSpecifier a, TypeSpecifier b)
        {
            if (ReferenceEquals(b, null))
            {
                return !ReferenceEquals(a, null);
            }

            return !a.Equals(b);
        }

        public static bool operator ==(TypeSpecifier a, GenericType b)
        {
            if (ReferenceEquals(b, null))
            {
                return ReferenceEquals(a, null);
            }

            return a.Equals(b);
        }

        public static bool operator !=(TypeSpecifier a, GenericType b)
        {
            if (ReferenceEquals(b, null))
            {
                return !ReferenceEquals(a, null);
            }

            return !a.Equals(b);
        }

        public static bool operator ==(TypeSpecifier a, BaseType b)
        {
            if (ReferenceEquals(b, null))
            {
                return ReferenceEquals(a, null);
            }

            return a.Equals(b);
        }

        public static bool operator !=(TypeSpecifier a, BaseType b)
        {
            if (ReferenceEquals(b, null))
            {
                return !ReferenceEquals(a, null);
            }

            return !a.Equals(b);
        }

        public static bool operator ==(BaseType a, TypeSpecifier b)
        {
            if (ReferenceEquals(b, null))
            {
                return ReferenceEquals(a, null);
            }

            return a.Equals(b);
        }

        public static bool operator !=(BaseType a, TypeSpecifier b)
        {
            if (ReferenceEquals(b, null))
            {
                return !ReferenceEquals(a, null);
            }

            return !a.Equals(b);
        }
    }
}
