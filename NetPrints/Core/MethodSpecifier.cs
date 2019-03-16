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
    /// Specifier describing a method.
    /// </summary>
    [Serializable]
    [DataContract]
    public class MethodSpecifier
    {
        /// <summary>
        /// Name of the method without any prefixes.
        /// </summary>
        [DataMember]
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Specifier for the type this method is contained in.
        /// </summary>
        [DataMember]
        public TypeSpecifier DeclaringType
        {
            get;
            private set;
        }

        /// <summary>
        /// Named specifiers for the types this method takes as arguments.
        /// </summary>
        [DataMember]
        public IList<Named<BaseType>> Arguments
        {
            get;
            private set;
        }

        /// <summary>
        /// Specifiers for the types this method takes as arguments.
        /// </summary>
        public IReadOnlyList<BaseType> ArgumentTypes
        {
            get => Arguments.Select(t => (BaseType)t).ToArray();
        }

        /// <summary>
        /// Specifiers for the types this method returns.
        /// </summary>
        [DataMember]
        public IList<BaseType> ReturnTypes
        {
            get;
            private set;
        }

        /// <summary>
        /// Modifiers this method has.
        /// </summary>
        [DataMember]
        public MethodModifiers Modifiers
        {
            get;
            private set;
        }

        /// <summary>
        /// Generic arguments this method takes.
        /// </summary>
        [DataMember]
        public IList<BaseType> GenericArguments
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a MethodSpecifier.
        /// </summary>
        /// <param name="name">Name of the method without any prefixes.</param>
        /// <param name="arguments">Specifiers for the arguments of the method.</param>
        /// <param name="returnTypes">Specifiers for the return types of the method.</param>
        /// <param name="modifiers">Modifiers of the method.</param>
        /// <param name="declaringType">Specifier for the type this method is contained in.</param>
        /// <param name="genericArguments">Generic arguments this method takes.</param>
        public MethodSpecifier(string name, IEnumerable<Named<BaseType>> arguments,
            IEnumerable<BaseType> returnTypes, MethodModifiers modifiers, TypeSpecifier declaringType,
            IList<BaseType> genericArguments)
        {
            Name = name;
            DeclaringType = declaringType;
            Arguments = arguments.ToList();
            ReturnTypes = returnTypes.ToList();
            Modifiers = modifiers;
            GenericArguments = genericArguments.ToList();
        }

        public override string ToString()
        {
            string methodString = "";

            if (Modifiers.HasFlag(MethodModifiers.Static))
            {
                methodString += "Static ";
            }

            string argTypeString = string.Join(", ", Arguments);

            methodString += $"{Name}({argTypeString})";

            if(GenericArguments.Count > 0)
            {
                string genArgTypeString = string.Join(", ", GenericArguments);
                methodString += $"<{genArgTypeString}>";
            }

            if(ReturnTypes.Count > 0)
            {
                string returnTypeString = string.Join(", ", ReturnTypes);
                methodString += $" : {returnTypeString}";
            }

            return methodString;
        }

        public override bool Equals(object obj)
        {
            if(obj is MethodSpecifier methodSpec)
            {
                return
                    methodSpec.Name == Name &&
                    methodSpec.DeclaringType == DeclaringType &&
                    methodSpec.ArgumentTypes.SequenceEqual(ArgumentTypes) &&
                    methodSpec.ReturnTypes.SequenceEqual(ReturnTypes) &&
                    methodSpec.Modifiers == Modifiers &&
                    methodSpec.GenericArguments.SequenceEqual(GenericArguments);
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator==(MethodSpecifier a, MethodSpecifier b)
        {
            if (ReferenceEquals(b, null))
            {
                return ReferenceEquals(a, null);
            }

            return a.Equals(b);
        }

        public static bool operator !=(MethodSpecifier a, MethodSpecifier b)
        {
            if (ReferenceEquals(b, null))
            {
                return !ReferenceEquals(a, null);
            }

            return !a.Equals(b);
        }
    }
}
