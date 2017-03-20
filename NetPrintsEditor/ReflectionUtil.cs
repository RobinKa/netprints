using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.ObjectModel;
using NetPrints.Core;
using NetPrintsEditor.Compilation;

namespace NetPrintsEditor
{
    public static class ReflectionUtil
    {
        public static ObservableRangeCollection<Type> NonStaticTypes = new ObservableRangeCollection<Type>();

        public static void UpdateNonStaticTypes(IEnumerable<Assembly> assemblies = null)
        {
            NonStaticTypes.ReplaceRange(GetNonStaticTypes(assemblies));
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

        public static IEnumerable<MethodInfo> GetStaticFunctionsWithReturnType(Type returnType, IEnumerable<Assembly> assemblies = null)
        {
            if (assemblies == null)
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }

            return assemblies.SelectMany(a =>
                a.GetTypes().Where(t => t.IsPublic).SelectMany(t =>
                    t.GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .Where(m => m.ReturnType == returnType)));
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

        public static IEnumerable<MethodInfo> GetPublicMethodsForType(Type t)
        {
            return t.GetMethods(BindingFlags.Instance | BindingFlags.Public);
        }
    }
}
