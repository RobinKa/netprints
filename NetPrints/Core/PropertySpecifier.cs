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
    [Serializable]
    [DataContract]
    public class PropertySpecifier
    {
        [DataMember]
        public string Name
        {
            get;
            private set;
        }

        [DataMember]
        public TypeSpecifier DeclaringType
        {
            get;
            private set;
        }

        [DataMember]
        public TypeSpecifier Type
        {
            get;
            private set;
        }

        [DataMember]
        public bool HasPublicGetter
        {
            get;
            private set;
        }

        [DataMember]
        public bool HasPublicSetter
        {
            get;
            private set;
        }
        
        public PropertySpecifier(string name, TypeSpecifier type, bool hasPublicGetter, 
            bool hasPublicSetter, TypeSpecifier declaringType)
        {
            Name = name;
            Type = type;
            HasPublicGetter = hasPublicGetter;
            HasPublicSetter = HasPublicSetter;
            DeclaringType = declaringType;
        }
    }
}
