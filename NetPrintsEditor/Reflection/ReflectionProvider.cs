using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NetPrints.Core;
using System.Xml;
using System.IO;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace NetPrintsEditor.Reflection
{
    public static class ISymbolExtensions
    {
        public static bool IsPublic(this ISymbol symbol)
        {
            return symbol.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Public;
        }

        public static IEnumerable<IMethodSymbol> GetMethods(this INamedTypeSymbol symbol)
        {
            return symbol.GetMembers()
                    .Where(member => member.Kind == SymbolKind.Method)
                    .Cast<IMethodSymbol>()
                    .Where(method => method.MethodKind == MethodKind.Ordinary);
        }

        public static bool IsSubclassOf(this ITypeSymbol symbol, ITypeSymbol cls)
        {
            // TODO: Traverse base types
            return cls?.BaseType == symbol;
        }

        public static string GetFullName(this ITypeSymbol typeSymbol)
        {
            string fullName = typeSymbol.MetadataName;
            if (typeSymbol.ContainingNamespace != null)
            {
                fullName = $"{typeSymbol.ContainingNamespace.MetadataName}.{fullName}";
            }
            return fullName;
        }
    }

    public class ReflectionProvider : IReflectionProvider
    {
        private readonly CSharpCompilation compilation;
        private readonly DocumentationUtil documentationUtil;

        public ReflectionProvider(IEnumerable<string> assemblyPaths)
        {
            compilation = CSharpCompilation.Create("C")
                .AddReferences(assemblyPaths.Select(path =>
                {
                    DocumentationProvider documentationProvider = DocumentationProvider.Default;

                    // Try to find the documentation in the framework doc path
                    string docPath = Path.ChangeExtension(path, ".xml");
                    if (!File.Exists(docPath))
                    {
                        docPath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                            "Reference Assemblies/Microsoft/Framework/.NETFramework/v4.X",
                            $"{Path.GetFileNameWithoutExtension(path)}.xml");
                    }

                    if (File.Exists(docPath))
                    {
                        documentationProvider = XmlDocumentationProvider.CreateFromFile(docPath);
                    }
                   
                    return MetadataReference.CreateFromFile(path, documentation: documentationProvider);
                }));

            documentationUtil = new DocumentationUtil(compilation);
        }

        private IEnumerable<INamedTypeSymbol> GetNamespaceTypes(INamespaceSymbol namespaceSymbol)
        {
            IEnumerable<INamedTypeSymbol> types = namespaceSymbol.GetTypeMembers();
            return types.Concat(namespaceSymbol.GetNamespaceMembers().SelectMany(ns => GetNamespaceTypes(ns)));
        }

        private IEnumerable<INamedTypeSymbol> GetValidTypes()
        {
            return compilation.SourceModule.ReferencedAssemblySymbols.SelectMany(module =>
                GetNamespaceTypes(module.GlobalNamespace));
        }

        private IEnumerable<INamedTypeSymbol> GetValidTypes(string name)
        {
            return compilation.SourceModule.ReferencedAssemblySymbols.Select(module =>
            {
                try { return module.GetTypeByMetadataName(name); }
                catch { return null; }
            }).Where(t => t != null);
        }

        private IEnumerable<INamedTypeSymbol> GetValidTypes(string name, int arity)
        {
            return GetValidTypes(name).Where(t => t.Arity == arity);
        }

        #region IReflectionProvider
        public IEnumerable<TypeSpecifier> GetNonStaticTypes()
        {
            return GetValidTypes().Where(
                t => t.IsPublic() && !(t.IsAbstract && t.IsSealed)).
                Select(t => ReflectionConverter.TypeSpecifierFromSymbol(t)).ToList();
        }

        public IEnumerable<MethodSpecifier> GetPublicMethodsForType(TypeSpecifier typeSpecifier)
        {
            INamedTypeSymbol type = GetTypeFromSpecifier(typeSpecifier);

            if (type != null)
            {
                // Get all public instance methods, ignore special ones (properties / events),
                // ignore those with generic parameters since we cant set those yet

                return type.GetMethods()
                    .Where(m => m.IsPublic() && !m.IsStatic && !m.IsGenericMethod)
                    .Select(m => ReflectionConverter.MethodSpecifierFromSymbol(m));
            }
            else
            {
                return new MethodSpecifier[] { };
            }
        }

        public IEnumerable<MethodSpecifier> GetPublicStaticFunctionsForType(TypeSpecifier typeSpecifier)
        {
            INamedTypeSymbol type = GetTypeFromSpecifier(typeSpecifier);

            if (type != null)
            {
                // Get all public static methods, ignore special ones (properties / events),
                // ignore those with generic parameters since we cant set those yet

                return type.GetMethods()
                    .Where(m => m.IsPublic() && !m.IsStatic && !m.IsGenericMethod)
                    .Select(m => ReflectionConverter.MethodSpecifierFromSymbol(m));
            }
            else
            {
                return new MethodSpecifier[] { };
            }
        }

        public IEnumerable<PropertySpecifier> GetPublicPropertiesForType(TypeSpecifier typeSpecifier)
        {
            return GetTypeFromSpecifier(typeSpecifier)
                .GetMembers()
                .Where(m => m.Kind == SymbolKind.Property)
                .Cast<IPropertySymbol>()
                .Select(p => ReflectionConverter.PropertySpecifierFromSymbol(p));
        }

        public IEnumerable<MethodSpecifier> GetStaticFunctions()
        {
            return GetValidTypes().Where(t => t.IsPublic()).SelectMany(t =>
                    t.GetMethods()
                    .Where(m => 
                        m.IsStatic && m.IsPublic() &&
                        m.Parameters.All(p => p.Type.TypeKind != TypeKind.TypeParameter) &&
                        m.ReturnType.TypeKind != TypeKind.TypeParameter &&
                        !m.IsGenericMethod &&
                        !m.ContainingType.IsUnboundGenericType)
                    .Select(m => ReflectionConverter.MethodSpecifierFromSymbol(m)));
        }
        
        public IEnumerable<ConstructorSpecifier> GetConstructors(TypeSpecifier typeSpecifier)
        {
            return GetTypeFromSpecifier(typeSpecifier)?.Constructors.Select(c => ReflectionConverter.ConstructorSpecifierFromSymbol(c));
        }

        public IEnumerable<string> GetEnumNames(TypeSpecifier typeSpecifier)
        {
            return GetTypeFromSpecifier(typeSpecifier).GetMembers()
                .Where(member => member.Kind == SymbolKind.Field)
                .Select(member => member.Name);
        }
        
        public bool TypeSpecifierIsSubclassOf(TypeSpecifier a, TypeSpecifier b)
        {
            INamedTypeSymbol typeA = GetTypeFromSpecifier(a);
            INamedTypeSymbol typeB = GetTypeFromSpecifier(b);

            return typeA != null && typeB != null && typeA.IsSubclassOf(typeB);
        }

        private INamedTypeSymbol GetTypeFromSpecifier(TypeSpecifier specifier)
        {
            string lookupName = specifier.Name;
            if (specifier.GenericArguments.Count > 0)
                lookupName += $"`{specifier.GenericArguments.Count}";

            IEnumerable<INamedTypeSymbol> types = GetValidTypes(lookupName);

            foreach (INamedTypeSymbol t in types)
            {
                if (t != null)
                {
                    if (specifier.GenericArguments.Count > 0)
                    {
                        var typeArguments = specifier.GenericArguments
                            .Select(baseType => baseType is TypeSpecifier typeSpec ?
                                GetTypeFromSpecifier(typeSpec) :
                                t.TypeArguments[specifier.GenericArguments.IndexOf(baseType)])
                            .ToArray();
                        return t.Construct(typeArguments);
                    }
                    else
                    {
                        return t;
                    }
                }
            }

            return null;
        }

        private IMethodSymbol GetMethodInfoFromSpecifier(MethodSpecifier specifier)
        {
            INamedTypeSymbol declaringType = GetTypeFromSpecifier(specifier.DeclaringType);
            return declaringType?.GetMethods().Where(
                    m => m.Name == specifier.Name && 
                    m.Parameters.Select(p => ReflectionConverter.BaseTypeSpecifierFromSymbol(p.Type)).SequenceEqual(specifier.Arguments))
                .FirstOrDefault();
        }

        public IEnumerable<MethodSpecifier> GetStaticFunctionsWithReturnType(TypeSpecifier searchTypeSpec)
        {
            // Find all public static methods

            IEnumerable<IMethodSymbol> availableMethods = GetValidTypes()
                        .Where(t => t.IsPublic())
                        .SelectMany(t =>
                            t.GetMethods()
                            .Where(m => m.IsPublic() && m.IsStatic)
                            .Where(m => !m.ContainingType.IsUnboundGenericType));

            INamedTypeSymbol searchType = GetTypeFromSpecifier(searchTypeSpec);

            List<MethodSpecifier> foundMethods = new List<MethodSpecifier>();

            // Find compatible methods

            foreach (IMethodSymbol availableMethod in availableMethods)
            {
                MethodSpecifier availableMethodSpec = ReflectionConverter.MethodSpecifierFromSymbol(availableMethod);

                // Check the return type whether it can be replaced by the wanted type

                ITypeSymbol retType = availableMethod.ReturnType;
                BaseType ret = ReflectionConverter.BaseTypeSpecifierFromSymbol(retType);

                if (ret == searchTypeSpec || retType.IsSubclassOf(searchType))
                {
                    MethodSpecifier foundMethod = ReflectionConverter.MethodSpecifierFromSymbol(availableMethod);
                    foundMethod = TryMakeClosedMethod(foundMethod, ret, searchTypeSpec);

                    // Only add fully closed methods
                    if (foundMethod != null && !foundMethods.Contains(foundMethod))
                    {
                        foundMethods.Add(foundMethod);
                    }
                }
                
            }

            return foundMethods;
        }

        public IEnumerable<MethodSpecifier> GetStaticFunctionsWithArgumentType(TypeSpecifier searchTypeSpec)
        {
            // Find all public static methods

            IEnumerable<IMethodSymbol> availableMethods = GetValidTypes()
                        .Where(t => t.IsPublic())
                        .SelectMany(t =>
                            t.GetMethods()
                            .Where(m => m.IsPublic() && m.IsStatic && !m.ContainingType.IsUnboundGenericType));

            INamedTypeSymbol searchType = GetTypeFromSpecifier(searchTypeSpec);

            List<MethodSpecifier> foundMethods = new List<MethodSpecifier>();

            // Find compatible methods

            foreach (IMethodSymbol availableMethod in availableMethods)
            {
                MethodSpecifier availableMethodSpec = ReflectionConverter.MethodSpecifierFromSymbol(availableMethod);

                // Check each argument whether it can be replaced by the wanted type
                for (int i = 0; i < availableMethodSpec.Arguments.Count; i++) 
                {
                    ITypeSymbol argType = availableMethod.Parameters[i].Type;
                    BaseType arg = ReflectionConverter.BaseTypeSpecifierFromSymbol(argType);

                    if (arg == searchTypeSpec || searchType.IsSubclassOf(argType))
                    {
                        MethodSpecifier foundMethod = ReflectionConverter.MethodSpecifierFromSymbol(availableMethod);
                        foundMethod = TryMakeClosedMethod(foundMethod, arg, searchTypeSpec);

                        // Only add fully closed methods
                        if (foundMethod != null && !foundMethods.Contains(foundMethod))
                        {
                            foundMethods.Add(foundMethod);
                        }
                    }
                }
            }

            return foundMethods;
        }

        private static MethodSpecifier TryMakeClosedMethod(MethodSpecifier method, 
            BaseType typeToReplace, TypeSpecifier replacementType)
        {
            // Create a list of generic types to replace with another type
            // These will then look for those generic types in the argument-
            // and return types and replace them with the new type

            Dictionary<GenericType, BaseType> replacedGenericTypes =
                    new Dictionary<GenericType, BaseType>();

            // If the arg is already generic itself, replace it directly with the 
            // passed type specifier
            // Otherwise, recursively check if the generic arguments should
            // be replaced

            if (typeToReplace is GenericType genType)
            {
                replacedGenericTypes.Add(genType, replacementType);
            }
            else if (typeToReplace is TypeSpecifier argTypeSpec)
            {
                if(!FindGenericArgumentsToReplace(argTypeSpec.GenericArguments,
                    replacementType.GenericArguments, ref replacedGenericTypes))
                {
                    return null;
                }
            }

            ReplaceGenericTypes(method.Arguments, replacedGenericTypes);
            ReplaceGenericTypes(method.ReturnTypes, replacedGenericTypes);

            // Remove the replaced generic arguments from the method
            replacedGenericTypes.Keys.ToList().ForEach(g =>
                method.GenericArguments.Remove(g));

            // Only add fully closed methods
            if (!method.GenericArguments.Any(a => a is GenericType))
            {
                return method;
            }

            return null;
        }

        /// <summary>
        /// Takes two topologically identical lists of types and 
        /// finds the replacement for the generic types of the first 
        /// with the type of the second at the same position
        /// </summary>
        private static bool FindGenericArgumentsToReplace(IList<BaseType> toReplace, 
                IList<BaseType> replaceWith, ref Dictionary<GenericType, BaseType> replacedGenericTypes)
        {
            for (int index = 0; index < toReplace.Count; index++)
            {
                // Replace the generic type directly if it is one
                // Otherwise, recursively replace generic arguments

                if (toReplace[index] is GenericType genType)
                {
                    // If there are two different types trying to replace
                    // the same generic parameter the method can not be
                    // made compatible
                    if (replacedGenericTypes.ContainsKey(genType) &&
                        replacedGenericTypes[genType] != replaceWith[index])
                    {
                        return false;
                    }

                    replacedGenericTypes.Add(genType, replaceWith[index]);
                }
                else if (toReplace[index] is TypeSpecifier typeSpec &&
                    replaceWith[index] is TypeSpecifier replaceWithTypeSpec)
                {
                    if (typeSpec.GenericArguments.Count != replaceWithTypeSpec.GenericArguments.Count)
                    {
                        throw new Exception();
                    }

                    for (int i = 0; i < typeSpec.GenericArguments.Count; i++)
                    {
                        if(!FindGenericArgumentsToReplace(typeSpec.GenericArguments,
                            replaceWithTypeSpec.GenericArguments, ref replacedGenericTypes))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Takes a list of types and a list of replacement types
        /// and replaces all types in the list with their replacements
        /// </summary>
        private static void ReplaceGenericTypes(IList<BaseType> types,
            Dictionary<GenericType, BaseType> replacedGenericTypes)
        {
            for(int i = 0; i < types.Count; i++)
            {
                BaseType t = types[i];

                if (t is GenericType genType)
                {
                    if (replacedGenericTypes.ContainsKey(genType))
                    {
                        types[i] = replacedGenericTypes[genType];
                    }
                }
                else if(t is TypeSpecifier typeSpec)
                {
                    ReplaceGenericTypes(typeSpec.GenericArguments, replacedGenericTypes);
                }
            }
        }

        // Documentation

        public string GetMethodDocumentation(MethodSpecifier methodSpecifier)
        {
            IMethodSymbol methodInfo = GetMethodInfoFromSpecifier(methodSpecifier);

            if(methodInfo == null)
            {
                return null;
            }

            return documentationUtil.GetMethodSummary(methodInfo);
        }

        public string GetMethodParameterDocumentation(MethodSpecifier methodSpecifier, int parameterIndex)
        {
            IMethodSymbol methodInfo = GetMethodInfoFromSpecifier(methodSpecifier);

            if (methodInfo == null)
            {
                return null;
            }

            return documentationUtil.GetMethodParameterInfo(methodInfo.Parameters[parameterIndex]);
        }

        public string GetMethodReturnDocumentation(MethodSpecifier methodSpecifier, int returnIndex)
        {
            IMethodSymbol methodInfo = GetMethodInfoFromSpecifier(methodSpecifier);

            if (methodInfo == null)
            {
                return null;
            }

            return documentationUtil.GetMethodReturnInfo(methodInfo);
        }

        #endregion
    }
}
