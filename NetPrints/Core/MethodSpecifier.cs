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
        public IList<BaseType> Arguments
        {
            get;
            private set;
        }

        [DataMember]
        public IList<BaseType> ReturnTypes
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

        [DataMember]
        public IList<BaseType> GenericArguments
        {
            get;
            private set;
        }

        public MethodSpecifier(string name, IEnumerable<BaseType> arguments,
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

            BaseType[] returnTypes;
            if(methodInfo.ReturnType.IsGenericParameter)
            {
                returnTypes = new BaseType[] { (GenericType)methodInfo.ReturnType };
            }
            else
            {
                returnTypes = methodInfo.ReturnType == typeof(void) ?
                    new BaseType[] { } :
                    new BaseType[] { (TypeSpecifier)methodInfo.ReturnType };
            }

            BaseType[] parameterTypes = methodInfo.GetParameters().Select(
                p => p.ParameterType.IsGenericParameter ?
                    ((GenericType)p.ParameterType) as BaseType :
                    ((TypeSpecifier)p.ParameterType) as BaseType).ToArray();

            return new MethodSpecifier(
                methodInfo.Name,
                parameterTypes,
                returnTypes,
                modifiers,
                methodInfo.DeclaringType);
        }
    }
}
