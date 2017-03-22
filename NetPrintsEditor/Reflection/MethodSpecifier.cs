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
    public class MethodSpecifier
    {
        public string Name
        {
            get;
        }
        
        public TypeSpecifier DeclaringType
        {
            get;
        }
        
        public IList<TypeSpecifier> Arguments
        {
            get;
        }
        
        public TypeSpecifier ReturnType
        {
            get;
        }
        
        public MethodModifiers Modifiers
        {
            get;
        }

        public MethodSpecifier(string name, IEnumerable<TypeSpecifier> arguments,
            TypeSpecifier returnType, MethodModifiers modifiers, TypeSpecifier declaringType)
        {
            Name = name;
            DeclaringType = declaringType;
            Arguments = arguments.ToList();
            ReturnType = returnType;
            Modifiers = modifiers;
        }

        public static implicit operator MethodSpecifier(MethodInfo methodInfo)
        {
            MethodModifiers modifiers = MethodModifiers.Private;

            if(methodInfo.IsPublic)
            {
                modifiers |= MethodModifiers.Public;
            }

            if(methodInfo.IsVirtual)
            {
                modifiers |= MethodModifiers.Virtual;
            }

            if(methodInfo.IsFinal)
            {
                modifiers |= MethodModifiers.Sealed;
            }

            if(methodInfo.IsAbstract)
            {
                modifiers |= MethodModifiers.Abstract;
            }

            if(methodInfo.IsStatic)
            {
                modifiers |= MethodModifiers.Static;
            }

            // TODO: Protected / Internal

            return new MethodSpecifier(
                methodInfo.Name,
                methodInfo.GetParameters().Select(p => (TypeSpecifier)p.ParameterType),
                methodInfo.ReturnType,
                modifiers,
                methodInfo.DeclaringType);
        }
    }
}
