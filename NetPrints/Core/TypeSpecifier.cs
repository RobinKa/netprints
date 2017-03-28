using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Core
{
    [DataContract]
    [Serializable]
    public class TypeSpecifier : BaseType
    {
        [DataMember]
        public bool IsEnum
        {
            get;
            private set;
        }

        [DataMember]
        public bool IsInterface
        {
            get;
            private set;
        }
        
        [DataMember]
        public ObservableRangeCollection<BaseType> GenericArguments
        {
            get;
            private set;
        }

        public override string ShortName
        {
            get
            {
                return Name.Split('.').Last();
            }
        }

        public bool IsPrimitive
        {
            get
            {
                return this == typeof(byte) || this == typeof(char) ||
                    this == typeof(short) || this == typeof(ushort) ||
                    this == typeof(int) || this == typeof(uint) ||
                    this == typeof(long) || this == typeof(ulong) ||
                    this == typeof(float) || this == typeof(double) ||
                    this == typeof(string) || this == typeof(bool) ||
                    IsEnum;
            }
        }
        
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

        public static TypeSpecifier Create<T>()
        {
            Type t = typeof(T);
            return new TypeSpecifier(t.FullName, t.IsSubclassOf(typeof(Enum)), t.IsInterface);
        }
        
        public override bool Equals(object obj)
        {
            if (obj is TypeSpecifier t)
            {
                // Name equal
                // Generic arguments equal
                // IsEnum equal

                if (Name == t.Name && GenericArguments.SequenceEqual(t.GenericArguments))
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

        public static implicit operator TypeSpecifier(Type type)
        {
            if (type.IsGenericParameter)
            {
                throw new ArgumentException(nameof(type));
            }

            string typeName = type.Name.Split('`').First();
            if(!string.IsNullOrEmpty(type.Namespace))
            {
                typeName = type.Namespace + "." + typeName;
            }

            TypeSpecifier typeSpecifier = new TypeSpecifier(typeName, 
                type.IsSubclassOf(typeof(Enum)),
                type.IsInterface);

            foreach(Type genType in type.GetGenericArguments())
            {
                if (genType.IsGenericParameter)
                {
                    // TODO: Convert and add constraints
                    typeSpecifier.GenericArguments.Add((GenericType)genType);
                }
                else
                {
                    typeSpecifier.GenericArguments.Add((TypeSpecifier)genType);
                }
            }
            
            return typeSpecifier;
        }

        public static implicit operator string(TypeSpecifier specifier)
        {
            return specifier.Name;
        }

        public static bool operator ==(TypeSpecifier a, TypeSpecifier b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(TypeSpecifier a, TypeSpecifier b)
        {
            return !a.Equals(b);
        }

        public static bool operator ==(TypeSpecifier a, GenericType b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(TypeSpecifier a, GenericType b)
        {
            return !a.Equals(b);
        }

        public static bool operator ==(TypeSpecifier typeSpecifier, Type type)
        {
            if(ReferenceEquals(type, null))
            {
                return ReferenceEquals(typeSpecifier, null);
            }

            if (type.IsGenericParameter)
            {
                return typeSpecifier.Equals((GenericType)type);
            }

            return typeSpecifier.Equals((TypeSpecifier)type);
        }

        public static bool operator !=(TypeSpecifier typeSpecifier, Type type)
        {
            if (ReferenceEquals(type, null))
            {
                return !ReferenceEquals(typeSpecifier, null);
            }

            if (type.IsGenericParameter)
            {
                return !typeSpecifier.Equals((GenericType)type);
            }

            return !typeSpecifier.Equals((TypeSpecifier)type);
        }

        public static bool operator ==(Type type, TypeSpecifier typeSpecifier)
        {
            if (ReferenceEquals(type, null))
            {
                return ReferenceEquals(typeSpecifier, null);
            }
            
            if(type.IsGenericParameter)
            {
                return typeSpecifier.Equals((GenericType)type);
            }

            return typeSpecifier.Equals((TypeSpecifier)type);
        }

        public static bool operator !=(Type type, TypeSpecifier typeSpecifier)
        {
            if (ReferenceEquals(type, null))
            {
                return !ReferenceEquals(typeSpecifier, null);
            }

            if (type.IsGenericParameter)
            {
                return !typeSpecifier.Equals((GenericType)type);
            }

            return !typeSpecifier.Equals((TypeSpecifier)type);
        }
    }
}
