using NetPrints.Core;
using System.Collections.Generic;

namespace NetPrintsEditor.Reflection
{
    public interface IReflectionProviderQuery
    {

    }

    public class ReflectionProviderMethodQuery : IReflectionProviderQuery
    {
        public TypeSpecifier Type { get; set; }
        public bool? Static { get; set; }
        public MemberVisibility? Visibility { get; set; }
        public TypeSpecifier ReturnType { get; set; }
        public TypeSpecifier ArgumentType { get; set; }

        public ReflectionProviderMethodQuery WithType(TypeSpecifier type)
        {
            Type = type;
            return this;
        }

        public ReflectionProviderMethodQuery WithStatic(bool isStatic)
        {
            Static = isStatic;
            return this;
        }

        public ReflectionProviderMethodQuery WithVisibility(MemberVisibility visibility)
        {
            Visibility = visibility;
            return this;
        }

        public ReflectionProviderMethodQuery WithReturnType(TypeSpecifier returnType)
        {
            ReturnType = returnType;
            return this;
        }

        public ReflectionProviderMethodQuery WithArgumentType(TypeSpecifier argumentType)
        {
            ArgumentType = argumentType;
            return this;
        }
    }

    public class ReflectionProviderPropertyQuery : IReflectionProviderQuery
    {
        public TypeSpecifier Type { get; set; }
        public bool? Static { get; set; }
        public MemberVisibility? Visibility { get; set; }
        public TypeSpecifier PropertyType { get; set; }
        public bool PropertyTypeDerivesFrom { get; set; } = false;

        public ReflectionProviderPropertyQuery WithType(TypeSpecifier type)
        {
            Type = type;
            return this;
        }

        public ReflectionProviderPropertyQuery WithStatic(bool isStatic)
        {
            Static = isStatic;
            return this;
        }

        public ReflectionProviderPropertyQuery WithVisibility(MemberVisibility visibility)
        {
            Visibility = visibility;
            return this;
        }

        public ReflectionProviderPropertyQuery WithPropertyType(TypeSpecifier type, bool derivesFrom = false)
        {
            PropertyType = type;
            PropertyTypeDerivesFrom = derivesFrom;
            return this;
        }
    }

    /// <summary>
    /// Interface for reflecting on types, methods etc.
    /// </summary>
    public interface IReflectionProvider
    {
        bool TypeSpecifierIsSubclassOf(TypeSpecifier a, TypeSpecifier b);
        bool HasImplicitCast(TypeSpecifier fromType, TypeSpecifier toType);
        IEnumerable<TypeSpecifier> GetNonStaticTypes();
        IEnumerable<MethodSpecifier> GetOverridableMethodsForType(TypeSpecifier typeSpecifier);
        IEnumerable<MethodSpecifier> GetPublicMethodOverloads(MethodSpecifier methodSpecifier);
        IEnumerable<ConstructorSpecifier> GetConstructors(TypeSpecifier typeSpecifier);
        IEnumerable<string> GetEnumNames(TypeSpecifier typeSpecifier);

        IEnumerable<MethodSpecifier> GetMethods(ReflectionProviderMethodQuery query);
        IEnumerable<PropertySpecifier> GetProperties(ReflectionProviderPropertyQuery query);

        // Documentation
        string GetMethodDocumentation(MethodSpecifier methodSpecifier);
        string GetMethodParameterDocumentation(MethodSpecifier methodSpecifier, int parameterIndex);
        string GetMethodReturnDocumentation(MethodSpecifier methodSpecifier, int returnIndex);
    }
}
