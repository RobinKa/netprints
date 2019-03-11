using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetPrints.Core
{
    /// <summary>
    /// Abstract specifier describing types.
    /// </summary>
    [Serializable]
    [DataContract]
    [KnownType(typeof(TypeSpecifier))]
    [KnownType(typeof(GenericType))]
    public abstract class BaseType
    {
        /// <summary>
        /// Full name of the type (ie. Namespace.TypeName).
        /// </summary>
        [DataMember]
        public string Name
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Short name of the type (ie. without namespace).
        /// </summary>
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
    }
}
