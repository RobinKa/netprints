using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Core
{
    [Serializable]
    [DataContract]
    [KnownType(typeof(TypeSpecifier))]
    [KnownType(typeof(GenericType))]
    public abstract class BaseType
    {
        [DataMember]
        public string Name
        {
            get;
            private set;
        }

        [DataMember]
        public virtual string ShortName
        {
            get => Name;
        }

        public BaseType(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public static implicit operator BaseType(Type type)
        {
            if (type.IsGenericParameter)
            {
                return (GenericType)type;
            }
            else
            {
                return (TypeSpecifier)type;
            }
        }
    }
}
