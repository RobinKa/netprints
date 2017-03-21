using NetPrints.Core;
using NetPrintsEditor.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetPrintsEditor
{
    public static class ReflectionUtil
    {
        public static ObservableRangeCollection<TypeSpecifier> NonStaticTypes = 
            new ObservableRangeCollection<TypeSpecifier>();

        public static bool TypeSpecifierIsSubclassOf(TypeSpecifier a, TypeSpecifier b, IEnumerable<Assembly> assemblies = null)
        {
            Type typeA = GetTypeFromSpecifier(a, assemblies);
            Type typeB = GetTypeFromSpecifier(b, assemblies);

            if(typeA != null && typeB != null)
            {
                return typeA.IsSubclassOf(typeB);
            }

            return false;
        }

        public static Type GetTypeFromSpecifier(TypeSpecifier specifier, IEnumerable<Assembly> assemblies = null)
        {
            if (assemblies == null)
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }

            foreach (Assembly assembly in assemblies)
            {
                Type t = assembly.GetType(specifier);
                if (t != null)
                {
                    return t;
                }
            }

            return null;
        }

        public static void UpdateNonStaticTypes(IEnumerable<Assembly> assemblies = null)
        {
            NonStaticTypes.ReplaceRange(GetNonStaticTypes(assemblies).Select(t => (TypeSpecifier)t));
        }

        public static IEnumerable<Assembly> LoadAssemblies(IEnumerable<LocalAssemblyName> assemblyNames)
        {
            return assemblyNames.Select(assemblyName => assemblyName.LoadAssembly()
                ?? throw new ArgumentException($"Could not load {assemblyName}"));
        }

        public static string GetAssemblyFullNameFromPath(string path)
        {
            Assembly assembly = Assembly.LoadFrom(path);
            return assembly.FullName;
        }

        public static IEnumerable<MethodInfo> GetStaticFunctions(IEnumerable<Assembly> assemblies = null)
        {
            if(assemblies == null)
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }

            return assemblies.SelectMany(a =>
                a.GetTypes().Where(t => t.IsPublic).SelectMany(t =>
                    t.GetMethods(BindingFlags.Static | BindingFlags.Public)));
        }

        public static IEnumerable<MethodInfo> GetStaticFunctionsWithReturnType(TypeSpecifier returnTypeSpecifier, IEnumerable<Assembly> assemblies = null)
        {
            if (assemblies == null)
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }

            return assemblies.SelectMany(a =>
                a.GetTypes().Where(t => t.IsPublic).SelectMany(t =>
                    t.GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .Where(m => m.ReturnType == returnTypeSpecifier)));
        }

        public static IEnumerable<Type> GetNonStaticTypes(IEnumerable<Assembly> assemblies = null)
        {
            if (assemblies == null)
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }

            return assemblies.SelectMany(a =>
                a.GetTypes().Where(t => t.IsPublic && !(t.IsAbstract && t.IsSealed)));
        }

        public static IEnumerable<MethodInfo> GetPublicMethodsForType(TypeSpecifier typeSpecifier, IEnumerable<Assembly> assemblies = null)
        {
            Type type = GetTypeFromSpecifier(typeSpecifier, assemblies);

            if (type != null)
            {
                return type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            }
            else
            {
                return new MethodInfo[] { };
            }
        }
    }
}
