using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NetPrints.Core;

namespace NetPrintsEditor.Reflection
{
    public class ReflectionProvider : IReflectionProvider
    {
        public List<Assembly> Assemblies
        {
            get;
            set;
        }
        
        public ReflectionProvider(List<Assembly> assemblies)
        {
            Assemblies = assemblies;
        }

        #region IReflectionProvider
        public IEnumerable<TypeSpecifier> GetNonStaticTypes()
        {
            return Assemblies.SelectMany(a => a.GetTypes().Where(
                t => t.IsPublic && !(t.IsAbstract && t.IsSealed)).
                Select(t => (TypeSpecifier)t));
        }

        public IEnumerable<MethodSpecifier> GetPublicMethodsForType(TypeSpecifier typeSpecifier)
        {
            Type type = GetTypeFromSpecifier(typeSpecifier);

            if (type != null)
            {
                // Get all public instance methods, ignore special ones (properties / events),
                // ignore those with generic parameters since we cant set those yet

                return type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(m => !m.IsSpecialName && !m.ContainsGenericParameters)
                    .Select(m => (MethodSpecifier)m);
            }
            else
            {
                return new MethodSpecifier[] { };
            }
        }

        public IEnumerable<PropertySpecifier> GetPublicPropertiesForType(TypeSpecifier typeSpecifier)
        {
            return GetTypeFromSpecifier(typeSpecifier)?.GetProperties().Select(p => (PropertySpecifier)p);
        }

        public IEnumerable<MethodSpecifier> GetStaticFunctions()
        {
            return Assemblies.SelectMany(a =>
                a.GetTypes().Where(t => t.IsPublic).SelectMany(t =>
                    t.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Where(m => 
                        !m.IsSpecialName && 
                        m.GetParameters().All(p => !p.ParameterType.IsGenericParameter) &&
                        !m.ReturnType.IsGenericParameter &&
                        !m.IsGenericMethodDefinition &&
                        !m.DeclaringType.ContainsGenericParameters)
                    .Select(m => (MethodSpecifier)m)));
        }
        
        public IEnumerable<ConstructorSpecifier> GetConstructors(TypeSpecifier typeSpecifier)
        {
            return GetTypeFromSpecifier(typeSpecifier)?.GetConstructors().Select(c => (ConstructorSpecifier)c);
        }

        public IEnumerable<string> GetEnumNames(TypeSpecifier typeSpecifier)
        {
            return GetTypeFromSpecifier(typeSpecifier)?.GetEnumNames();
        }
        
        public bool TypeSpecifierIsSubclassOf(TypeSpecifier a, TypeSpecifier b)
        {
            Type typeA = GetTypeFromSpecifier(a);
            Type typeB = GetTypeFromSpecifier(b);

            return typeA != null && typeB != null && typeA.IsSubclassOf(typeB);
        }

        private Type GetTypeFromSpecifier(TypeSpecifier specifier)
        {
            foreach (Assembly assembly in Assemblies)
            {
                string typeName = specifier.Name;
                if(specifier.GenericArguments.Count > 0)
                {
                    typeName += $"`{specifier.GenericArguments.Count}";
                }

                Type t = assembly.GetType(typeName);
                if (t != null)
                {
                    if (specifier.GenericArguments.Count > 0 && t.IsGenericTypeDefinition)
                    {
                        return t.MakeGenericType(specifier.GenericArguments
                            .Select(baseType => baseType is TypeSpecifier typeSpec ?
                                GetTypeFromSpecifier(typeSpec) :
                                t.GetGenericArguments()[specifier.GenericArguments.IndexOf(baseType)])
                            .ToArray());
                    }
                    else
                    {
                        return t;
                    }
                }
            }

            return null;
        }

        public IEnumerable<MethodSpecifier> GetStaticFunctionsWithReturnType(TypeSpecifier searchTypeSpec)
        {
            // Find all public static methods

            IEnumerable<MethodInfo> availableMethods = Assemblies
                .SelectMany(a =>
                    a.GetTypes()
                        .Where(t => t.IsPublic)
                        .SelectMany(t =>
                            t.GetMethods(BindingFlags.Static | BindingFlags.Public)
                            .Where(m => !m.IsSpecialName &&
                                !m.DeclaringType.ContainsGenericParameters)));

            Type searchType = GetTypeFromSpecifier(searchTypeSpec);

            List<MethodSpecifier> foundMethods = new List<MethodSpecifier>();

            // Find compatible methods

            foreach (MethodInfo availableMethod in availableMethods)
            {
                MethodSpecifier availableMethodSpec = availableMethod;

                // Check the return type whether it can be replaced by the wanted type

                Type retType = availableMethod.ReturnType;
                BaseType ret = retType;

                if (ret == searchTypeSpec || retType.IsSubclassOf(searchType))
                {
                    MethodSpecifier foundMethod = availableMethod;
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

            IEnumerable<MethodInfo> availableMethods = Assemblies
                .SelectMany(a =>
                    a.GetTypes()
                        .Where(t => t.IsPublic)
                        .SelectMany(t =>
                            t.GetMethods(BindingFlags.Static | BindingFlags.Public)
                            .Where(m => !m.IsSpecialName && 
                                !m.DeclaringType.ContainsGenericParameters)));

            Type searchType = GetTypeFromSpecifier(searchTypeSpec);

            List<MethodSpecifier> foundMethods = new List<MethodSpecifier>();

            // Find compatible methods

            foreach(MethodInfo availableMethod in availableMethods)
            {
                MethodSpecifier availableMethodSpec = availableMethod;

                // Check each argument whether it can be replaced by the wanted type
                for(int i = 0; i < availableMethodSpec.Arguments.Count; i++) 
                {
                    Type argType = availableMethod.GetParameters()[i].ParameterType;
                    BaseType arg = argType;

                    if (arg == searchTypeSpec || searchType.IsSubclassOf(argType))
                    {
                        MethodSpecifier foundMethod = availableMethod;
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
                FindGenericArgumentsToReplace(argTypeSpec.GenericArguments,
                    replacementType.GenericArguments, ref replacedGenericTypes);
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
        private static void FindGenericArgumentsToReplace(IList<BaseType> toReplace, 
                IList<BaseType> replaceWith, ref Dictionary<GenericType, BaseType> replacedGenericTypes)
        {
            for (int index = 0; index < toReplace.Count; index++)
            {
                // Replace the generic type directly if it is one
                // Otherwise, recursively replace generic arguments

                if (toReplace[index] is GenericType genType)
                {
                    if (replacedGenericTypes.ContainsKey(genType) &&
                        replacedGenericTypes[genType] != replaceWith[index])
                    {
                        throw new Exception();
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
                        FindGenericArgumentsToReplace(typeSpec.GenericArguments,
                            replaceWithTypeSpec.GenericArguments, ref replacedGenericTypes);
                    }
                }
            }
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

        #endregion
    }
}
