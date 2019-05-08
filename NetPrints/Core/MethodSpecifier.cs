using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NetPrints.Core
{
    public enum MethodParameterPassType
    {
        Default,
        Reference,
        Out,
        In
    }

    [DataContract]
    public class MethodParameter : Named<BaseType>
    {
        [DataMember]
        public MethodParameterPassType PassType
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether the parameter has an explicit default value.
        /// </summary>
        [DataMember]
        public bool HasExplicitDefaultValue
        {
            get;
            private set;
        }

        /// <summary>
        /// Explicit default value for the parameter.
        /// Only valid when HasExplicitDefaultValue is true.
        /// </summary>
        [DataMember]
        public object ExplicitDefaultValue
        {
            get;
            private set;
        }

        public MethodParameter(string name, BaseType type, MethodParameterPassType passType,
            bool hasExplicitDefaultValue, object explicitDefaultValue)
            : base(name, type)
        {
            PassType = passType;
            HasExplicitDefaultValue = hasExplicitDefaultValue;
            ExplicitDefaultValue = explicitDefaultValue;
        }
    }

    /// <summary>
    /// Specifier describing a method.
    /// </summary>
    [Serializable]
    [DataContract]
    public partial class MethodSpecifier
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
        public IList<MethodParameter> Parameters
        {
            get;
            private set;
        }

        /// <summary>
        /// Specifiers for the types this method takes as arguments.
        /// </summary>
        public IReadOnlyList<BaseType> ArgumentTypes
        {
            get => Parameters.Select(t => (BaseType)t).ToArray();
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
        /// Visibility of this method.
        /// </summary>
        [DataMember]
        public MemberVisibility Visibility
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
        public MethodSpecifier(string name, IEnumerable<MethodParameter> arguments,
            IEnumerable<BaseType> returnTypes, MethodModifiers modifiers, MemberVisibility visibility, TypeSpecifier declaringType,
            IList<BaseType> genericArguments)
        {
            Name = name;
            DeclaringType = declaringType;
            Parameters = arguments.ToList();
            ReturnTypes = returnTypes.ToList();
            Modifiers = modifiers;
            Visibility = visibility;
            GenericArguments = genericArguments.ToList();
        }

        public override string ToString()
        {
            string methodString = "";

            if (Modifiers.HasFlag(MethodModifiers.Static))
            {
                methodString += $"{DeclaringType.ShortName}.";
            }

            methodString += Name;

            string argTypeString = string.Join(", ", Parameters.Select(a => a.Value.ShortName));

            methodString += $"({argTypeString})";

            if (GenericArguments.Count > 0)
            {
                string genArgTypeString = string.Join(", ", GenericArguments.Select(s => s.ShortName));
                methodString += $"<{genArgTypeString}>";
            }

            if (ReturnTypes.Count > 0)
            {
                string returnTypeString = string.Join(", ", ReturnTypes.Select(s => s.ShortName));
                methodString += $" : {returnTypeString}";
            }

            return methodString;
        }

        public override bool Equals(object obj)
        {
            if (obj is MethodSpecifier methodSpec)
            {
                return
                    methodSpec.Name == Name
                    && methodSpec.DeclaringType == DeclaringType
                    && methodSpec.ArgumentTypes.SequenceEqual(ArgumentTypes)
                    && methodSpec.ReturnTypes.SequenceEqual(ReturnTypes)
                    && methodSpec.Modifiers == Modifiers
                    && methodSpec.GenericArguments.SequenceEqual(GenericArguments);
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Modifiers, string.Join(",", GenericArguments), string.Join(",", ReturnTypes), string.Join(",", Parameters), Visibility, DeclaringType);
        }

        public static bool operator==(MethodSpecifier a, MethodSpecifier b)
        {
            if (a is null)
            {
                return b is null;
            }

            return a.Equals(b);
        }

        public static bool operator !=(MethodSpecifier a, MethodSpecifier b)
        {
            if (a is null)
            {
                return !(b is null);
            }

            return !a.Equals(b);
        }
    }
}
