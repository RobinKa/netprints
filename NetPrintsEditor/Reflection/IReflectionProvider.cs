using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrintsEditor.Reflection
{
    /// <summary>
    /// Interface for reflecting on types, methods etc.
    /// </summary>
    public interface IReflectionProvider
    {
        bool TypeSpecifierIsSubclassOf(TypeSpecifier a, TypeSpecifier b);
        bool HasImplicitCast(TypeSpecifier fromType, TypeSpecifier toType);
        IEnumerable<MethodSpecifier> GetStaticFunctions();
        IEnumerable<MethodSpecifier> GetStaticFunctionsWithReturnType(TypeSpecifier returnTypeSpecifier);
        IEnumerable<MethodSpecifier> GetStaticFunctionsWithArgumentType(TypeSpecifier typeSpecifier);
        IEnumerable<TypeSpecifier> GetNonStaticTypes();
        IEnumerable<MethodSpecifier> GetPublicMethodsForType(TypeSpecifier typeSpecifier);
        IEnumerable<MethodSpecifier> GetPublicMethodOverloads(MethodSpecifier methodSpecifier);
        IEnumerable<MethodSpecifier> GetPublicStaticFunctionsForType(TypeSpecifier typeSpecifier);
        IEnumerable<PropertySpecifier> GetPublicPropertiesForType(TypeSpecifier typeSpecifier);
        IEnumerable<ConstructorSpecifier> GetConstructors(TypeSpecifier typeSpecifier);
        IEnumerable<string> GetEnumNames(TypeSpecifier typeSpecifier);

        // Documentation
        string GetMethodDocumentation(MethodSpecifier methodSpecifier);
        string GetMethodParameterDocumentation(MethodSpecifier methodSpecifier, int parameterIndex);
        string GetMethodReturnDocumentation(MethodSpecifier methodSpecifier, int returnIndex);
    }
}
