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
        public bool? HasGenericArguments { get; set; }

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

        public ReflectionProviderMethodQuery WithHasGenericArguments(bool hasGenericArguments)
        {
            HasGenericArguments = hasGenericArguments;
            return this;
        }
    }

    public class ReflectionProviderVariableQuery : IReflectionProviderQuery
    {
        public TypeSpecifier Type { get; set; }
        public bool? Static { get; set; }
        public MemberVisibility? Visibility { get; set; }
        public TypeSpecifier VariableType { get; set; }
        public bool VariableTypeDerivesFrom { get; set; } = false;

        public ReflectionProviderVariableQuery WithType(TypeSpecifier type)
        {
            Type = type;
            return this;
        }

        public ReflectionProviderVariableQuery WithStatic(bool isStatic)
        {
            Static = isStatic;
            return this;
        }

        public ReflectionProviderVariableQuery WithVisibility(MemberVisibility visibility)
        {
            Visibility = visibility;
            return this;
        }

        public ReflectionProviderVariableQuery WithVariableType(TypeSpecifier type, bool derivesFrom = false)
        {
            VariableType = type;
            VariableTypeDerivesFrom = derivesFrom;
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
        IEnumerable<VariableSpecifier> GetVariables(ReflectionProviderVariableQuery query);

        // Documentation
        string GetMethodDocumentation(MethodSpecifier methodSpecifier);
        string GetMethodParameterDocumentation(MethodSpecifier methodSpecifier, int parameterIndex);
        string GetMethodReturnDocumentation(MethodSpecifier methodSpecifier, int returnIndex);
    }
}
