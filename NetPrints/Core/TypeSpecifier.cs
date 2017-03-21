using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Core
{
    [DataContract]
    public class TypeSpecifier
    {
        [DataMember]
        public string Name
        {
            get;
            set;
        }

        public string ShortName
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
                    this == typeof(string) || this == typeof(bool);
            }
        }
        
        public TypeSpecifier(string typeName)
        {
            Name = typeName;
        }

        public static TypeSpecifier Create<T>()
        {
            return typeof(T);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if(obj is TypeSpecifier t)
            {
                return Name == t.Name;
            }

            return false;
        }

        public override string ToString()
        {
            return Name;
        }

        public static implicit operator TypeSpecifier(string typeName)
        {
            return new TypeSpecifier(typeName);
        }

        public static implicit operator TypeSpecifier(Type type)
        {
            return new TypeSpecifier(type.FullName);
        }

        public static implicit operator string(TypeSpecifier specifier)
        {
            return specifier.Name;
        }

        public static bool operator ==(TypeSpecifier typeSpecifier, Type type)
        {
            if(ReferenceEquals(type, null))
            {
                return ReferenceEquals(typeSpecifier, null);
            }

            return typeSpecifier.Equals((TypeSpecifier)type);
        }

        public static bool operator !=(TypeSpecifier typeSpecifier, Type type)
        {
            if (ReferenceEquals(type, null))
            {
                return !ReferenceEquals(typeSpecifier, null);
            }

            return !typeSpecifier.Equals((TypeSpecifier)type);
        }

        public static bool operator ==(Type type, TypeSpecifier typeSpecifier)
        {
            if (ReferenceEquals(type, null))
            {
                return ReferenceEquals(typeSpecifier, null);
            }

            return typeSpecifier.Equals((TypeSpecifier)type);
        }

        public static bool operator !=(Type type, TypeSpecifier typeSpecifier)
        {
            if (ReferenceEquals(type, null))
            {
                return !ReferenceEquals(typeSpecifier, null);
            }

            return !typeSpecifier.Equals((TypeSpecifier)type);
        }

        public static bool operator ==(TypeSpecifier a, TypeSpecifier b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(TypeSpecifier a, TypeSpecifier b)
        {
            return !a.Equals(b);
        }
    }
}
