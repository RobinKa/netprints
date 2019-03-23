using System;
using System.Collections.Generic;
using NetPrints.Core;

namespace NetPrintsEditor.Reflection
{
    public class MemoizedReflectionProvider : IReflectionProvider
    {
        private IReflectionProvider provider;

        private Func<TypeSpecifier, IEnumerable<ConstructorSpecifier>> memoizedGetConstructors;
        private Func<TypeSpecifier, IEnumerable<string>> memoizedGetEnumNames;
        private Func<MethodSpecifier, string> memoizedGetMethodDocumentation;
        private Func<MethodSpecifier, int, string> memoizedGetMethodParameterDocumentation;
        private Func<MethodSpecifier, int, string> memoizedGetMethodReturnDocumentation;
        private Func<IEnumerable<TypeSpecifier>> memoizedGetNonStaticTypes;
        private Func<TypeSpecifier, IEnumerable<MethodSpecifier>> memoizedGetOverridableMethodsForType;
        private Func<TypeSpecifier, IEnumerable<MethodSpecifier>> memoizedGetProtectedMethodsForType;
        private Func<MethodSpecifier, IEnumerable<MethodSpecifier>> memoizedGetPublicMethodOverloads;
        private Func<TypeSpecifier, IEnumerable<MethodSpecifier>> memoizedGetPublicMethodsForType;
        private Func<TypeSpecifier, IEnumerable<PropertySpecifier>> memoizedGetPublicPropertiesForType;
        private Func<TypeSpecifier, IEnumerable<MethodSpecifier>> memoizedGetPublicStaticFunctionsForType;
        private Func<IEnumerable<PropertySpecifier>> memoizedGetPublicStaticProperties;
        private Func<IEnumerable<MethodSpecifier>> memoizedGetStaticFunctions;
        private Func<TypeSpecifier, IEnumerable<MethodSpecifier>> memoizedGetStaticFunctionsWithArgumentType;
        private Func<TypeSpecifier, IEnumerable<MethodSpecifier>> memoizedGetStaticFunctionsWithReturnType;
        private Func<TypeSpecifier, TypeSpecifier, bool> memoizedHasImplicitCast;
        private Func<TypeSpecifier, TypeSpecifier, bool> memoizedTypeSpecifierIsSubclassOf;

        public MemoizedReflectionProvider(IReflectionProvider reflectionProvider)
        {
            provider = reflectionProvider;

            Reset();
        }

        /// <summary>
        /// Resets the memoization.
        /// </summary>
        public void Reset()
        {
            memoizedGetConstructors = provider.GetConstructors;
            memoizedGetConstructors = memoizedGetConstructors.Memoize();

            memoizedGetEnumNames = provider.GetEnumNames;
            memoizedGetEnumNames = memoizedGetEnumNames.Memoize();

            memoizedGetMethodDocumentation = provider.GetMethodDocumentation;
            memoizedGetMethodDocumentation = memoizedGetMethodDocumentation.Memoize();

            memoizedGetMethodParameterDocumentation = provider.GetMethodParameterDocumentation;
            memoizedGetMethodParameterDocumentation = memoizedGetMethodParameterDocumentation.Memoize();

            memoizedGetMethodReturnDocumentation = provider.GetMethodReturnDocumentation;
            memoizedGetMethodReturnDocumentation = memoizedGetMethodReturnDocumentation.Memoize();

            memoizedGetNonStaticTypes = provider.GetNonStaticTypes;
            memoizedGetNonStaticTypes = memoizedGetNonStaticTypes.Memoize();

            memoizedGetOverridableMethodsForType = provider.GetOverridableMethodsForType;
            memoizedGetOverridableMethodsForType = memoizedGetOverridableMethodsForType.Memoize();

            memoizedGetProtectedMethodsForType = provider.GetProtectedMethodsForType;
            memoizedGetProtectedMethodsForType = memoizedGetProtectedMethodsForType.Memoize();

            memoizedGetPublicMethodOverloads = provider.GetPublicMethodOverloads;
            memoizedGetPublicMethodOverloads = memoizedGetPublicMethodOverloads.Memoize();

            memoizedGetPublicMethodsForType = provider.GetPublicMethodsForType;
            memoizedGetPublicMethodsForType = memoizedGetPublicMethodsForType.Memoize();

            memoizedGetPublicPropertiesForType = provider.GetPublicPropertiesForType;
            memoizedGetPublicPropertiesForType = memoizedGetPublicPropertiesForType.Memoize();

            memoizedGetPublicStaticFunctionsForType = provider.GetPublicStaticFunctionsForType;
            memoizedGetPublicStaticFunctionsForType = memoizedGetPublicStaticFunctionsForType.Memoize();

            memoizedGetPublicStaticProperties = provider.GetPublicStaticProperties;
            memoizedGetPublicStaticProperties = memoizedGetPublicStaticProperties.Memoize();

            memoizedGetStaticFunctions = provider.GetStaticFunctions;
            memoizedGetStaticFunctions = memoizedGetStaticFunctions.Memoize();

            memoizedGetStaticFunctionsWithArgumentType = provider.GetStaticFunctionsWithArgumentType;
            memoizedGetStaticFunctionsWithArgumentType = memoizedGetStaticFunctionsWithArgumentType.Memoize();

            memoizedGetStaticFunctionsWithReturnType = provider.GetStaticFunctionsWithReturnType;
            memoizedGetStaticFunctionsWithReturnType = memoizedGetStaticFunctionsWithReturnType.Memoize();

            memoizedHasImplicitCast = provider.HasImplicitCast;
            memoizedHasImplicitCast = memoizedHasImplicitCast.Memoize();

            memoizedTypeSpecifierIsSubclassOf = provider.TypeSpecifierIsSubclassOf;
            memoizedTypeSpecifierIsSubclassOf = memoizedTypeSpecifierIsSubclassOf.Memoize();
        }

        public IEnumerable<ConstructorSpecifier> GetConstructors(TypeSpecifier typeSpecifier)
            => memoizedGetConstructors(typeSpecifier);

        public IEnumerable<string> GetEnumNames(TypeSpecifier typeSpecifier)
            => memoizedGetEnumNames(typeSpecifier);

        public string GetMethodDocumentation(MethodSpecifier methodSpecifier)
            => memoizedGetMethodDocumentation(methodSpecifier);

        public string GetMethodParameterDocumentation(MethodSpecifier methodSpecifier, int parameterIndex)
            => memoizedGetMethodParameterDocumentation(methodSpecifier, parameterIndex);

        public string GetMethodReturnDocumentation(MethodSpecifier methodSpecifier, int returnIndex)
            => memoizedGetMethodReturnDocumentation(methodSpecifier, returnIndex);

        public IEnumerable<TypeSpecifier> GetNonStaticTypes()
            => memoizedGetNonStaticTypes();

        public IEnumerable<MethodSpecifier> GetOverridableMethodsForType(TypeSpecifier typeSpecifier)
            => memoizedGetOverridableMethodsForType(typeSpecifier);

        public IEnumerable<MethodSpecifier> GetProtectedMethodsForType(TypeSpecifier typeSpecifier)
            => memoizedGetProtectedMethodsForType(typeSpecifier);

        public IEnumerable<MethodSpecifier> GetPublicMethodOverloads(MethodSpecifier methodSpecifier)
            => memoizedGetPublicMethodOverloads(methodSpecifier);

        public IEnumerable<MethodSpecifier> GetPublicMethodsForType(TypeSpecifier typeSpecifier)
            => memoizedGetPublicMethodsForType(typeSpecifier);

        public IEnumerable<PropertySpecifier> GetPublicPropertiesForType(TypeSpecifier typeSpecifier)
            => memoizedGetPublicPropertiesForType(typeSpecifier);

        public IEnumerable<MethodSpecifier> GetPublicStaticFunctionsForType(TypeSpecifier typeSpecifier)
            => memoizedGetPublicStaticFunctionsForType(typeSpecifier);

        public IEnumerable<PropertySpecifier> GetPublicStaticProperties()
            => memoizedGetPublicStaticProperties();

        public IEnumerable<MethodSpecifier> GetStaticFunctions()
            => memoizedGetStaticFunctions();

        public IEnumerable<MethodSpecifier> GetStaticFunctionsWithArgumentType(TypeSpecifier typeSpecifier)
            => memoizedGetStaticFunctionsWithArgumentType(typeSpecifier);

        public IEnumerable<MethodSpecifier> GetStaticFunctionsWithReturnType(TypeSpecifier returnTypeSpecifier)
            => memoizedGetStaticFunctionsWithReturnType(returnTypeSpecifier);

        public bool HasImplicitCast(TypeSpecifier fromType, TypeSpecifier toType)
            => memoizedHasImplicitCast(fromType, toType);

        public bool TypeSpecifierIsSubclassOf(TypeSpecifier a, TypeSpecifier b)
            => memoizedTypeSpecifierIsSubclassOf(a, b);
    }
}
