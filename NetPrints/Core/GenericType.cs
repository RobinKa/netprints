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
    public class GenericTypeConstraint
    {
        
    }

    [DataContract]
    [Serializable]
    public class GenericType : BaseType
    {
        public ObservableRangeCollection<GenericTypeConstraint> Constraints
        {
            get;
            private set;
        }

        public GenericType(string name, IEnumerable<GenericTypeConstraint> constraints = null)
            : base(name)
        {
            if(constraints == null)
            {
                Constraints = new ObservableRangeCollection<GenericTypeConstraint>();
            }
            else
            {
                Constraints = new ObservableRangeCollection<GenericTypeConstraint>(constraints);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is TypeSpecifier t)
            {
                // TODO: Check constraints
                return true;
            }
            else if (obj is GenericType genType)
            {
                // TODO: Check constraints
                return Name == genType.Name;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(GenericType a, GenericType b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(GenericType a, GenericType b)
        {
            return !a.Equals(b);
        }

        public static bool operator ==(GenericType a, TypeSpecifier b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(GenericType a, TypeSpecifier b)
        {
            return !a.Equals(b);
        }
    }
}
