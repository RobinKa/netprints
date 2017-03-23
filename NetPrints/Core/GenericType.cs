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

        public static implicit operator GenericType(Type type)
        {
            if (!type.IsGenericParameter)
            {
                throw new ArgumentException(nameof(type));
            }
            
            GenericType genericType = new GenericType(type.Name);

            return genericType;
        }
    }
}
