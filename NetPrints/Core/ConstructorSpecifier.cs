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
    /// <summary>
    /// Specifier describing a constructor.
    /// </summary>
    [Serializable]
    public class ConstructorSpecifier
    {
        /// <summary>
        /// Specifier for the type this constructor is for.
        /// </summary>
        public TypeSpecifier DeclaringType
        {
            get;
        }
        
        /// <summary>
        /// Specifiers for the arguments this constructor takes.
        /// </summary>
        public IList<Named<BaseType>> Arguments
        {
            get;
        }

        /// <summary>
        /// Creates a ConstructorSpecifier given specifiers for the constructor's arguments and the type it is for.
        /// </summary>
        /// <param name="arguments">Specifiers for the arguments the constructor takes.</param>
        /// <param name="declaringType">Specifier for the type the constructor is for.</param>
        public ConstructorSpecifier(IEnumerable<Named<BaseType>> arguments, TypeSpecifier declaringType)
        {
            DeclaringType = declaringType;
            Arguments = arguments.ToList();
        }

        public override string ToString()
        {
            string constructorString = "";

            string argTypeString = string.Join(", ", Arguments);

            constructorString += $"{DeclaringType.Name}({argTypeString})";

            return constructorString;
        }
    }
}
