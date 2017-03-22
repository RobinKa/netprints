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
    public class MethodSpecifier
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
        public IList<TypeSpecifier> Arguments
        {
            get;
            private set;
        }

        [DataMember]
        public IList<TypeSpecifier> ReturnTypes
        {
            get;
            private set;
        }

        [DataMember]
        public MethodModifiers Modifiers
        {
            get;
            private set;
        }

        public MethodSpecifier(string name, IEnumerable<TypeSpecifier> arguments,
            IEnumerable<TypeSpecifier> returnTypes, MethodModifiers modifiers, TypeSpecifier declaringType)
        {
            Name = name;
            DeclaringType = declaringType;
            Arguments = arguments.ToList();
            ReturnTypes = returnTypes.ToList();
            Modifiers = modifiers;
        }

        public static implicit operator MethodSpecifier(MethodInfo methodInfo)
        {
            MethodModifiers modifiers = MethodModifiers.Private;

            if (methodInfo.IsPublic)
            {
                modifiers |= MethodModifiers.Public;
            }

            if (methodInfo.IsVirtual)
            {
                modifiers |= MethodModifiers.Virtual;
            }

            if (methodInfo.IsFinal)
            {
                modifiers |= MethodModifiers.Sealed;
            }

            if (methodInfo.IsAbstract)
            {
                modifiers |= MethodModifiers.Abstract;
            }

            if (methodInfo.IsStatic)
            {
                modifiers |= MethodModifiers.Static;
            }

            // TODO: Protected / Internal

            TypeSpecifier[] returnTypes = methodInfo.ReturnType == typeof(void) ?
                new TypeSpecifier[] { } :
                new TypeSpecifier[] { methodInfo.ReturnType };

            return new MethodSpecifier(
                methodInfo.Name,
                methodInfo.GetParameters().Select(p => (TypeSpecifier)p.ParameterType),
                returnTypes,
                modifiers,
                methodInfo.DeclaringType);
        }
    }
}
