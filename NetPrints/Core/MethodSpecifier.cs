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
                    methodSpec.Arguments.SequenceEqual(Arguments) &&
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
