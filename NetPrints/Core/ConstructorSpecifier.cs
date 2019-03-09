using Microsoft.CodeAnalysis;
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
    }
}
