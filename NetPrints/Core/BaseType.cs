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
        /// Full name of the type as it would appear in code.
        /// In addition to specifying generic arguments, the difference to Name
        /// is that nested classes have a "+" in the backend, while they have a "."
        /// when writing them in code.
        /// </summary>
        public virtual string FullCodeName
        {
            get => Name.Replace("+", ".");
        }

        /// <summary>
        /// Same as <see cref="FullCodeName"/> but with unbound generic arguments replaced
        /// by blank (eg. List<T> -> List<>). Needed when referring to unbound types in code.
        /// </summary>
        public virtual string FullCodeNameUnbound
        {
            get => Name.Replace("+", ".");
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
