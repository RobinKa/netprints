using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetPrintsEditor.Reflection
{
    [Serializable]
    public class ConstructorSpecifier
    {
        public TypeSpecifier DeclaringType
        {
            get;
        }
        
        public IList<TypeSpecifier> Arguments
        {
            get;
        }

        public ConstructorSpecifier(IEnumerable<TypeSpecifier> arguments, TypeSpecifier declaringType)
        {
            DeclaringType = declaringType;
            Arguments = arguments.ToList();
        }

        public static implicit operator ConstructorSpecifier(ConstructorInfo constructorInfo)
        {
            return new ConstructorSpecifier(
                constructorInfo.GetParameters().Select(p => (TypeSpecifier)p.ParameterType),
                constructorInfo.DeclaringType);
        }
    }
}
