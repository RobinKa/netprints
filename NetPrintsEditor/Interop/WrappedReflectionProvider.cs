using NetPrints.Core;
using NetPrintsEditor.Compilation;
using NetPrintsEditor.Interop;
using NetPrintsEditor.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetPrintsEditor.Interop
{
    public class WrappedReflectionProvider : WrappedAppDomainObject, IReflectionProvider
    {
        private IReflectionProvider reflectionProvider;

        public void SetReflectionAssemblies(IEnumerable<string> assemblyPaths)
        {
            List<Assembly> assemblies = new List<Assembly>();

            foreach (string assemblyPath in assemblyPaths)
            {
                assemblies.Add(Assembly.LoadFrom(assemblyPath));
            }

            reflectionProvider = new ReflectionProvider(assemblies);
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
        
        #region IReflectionProvider
        public bool TypeSpecifierIsSubclassOf(TypeSpecifier a, TypeSpecifier b)
        {
            return reflectionProvider.TypeSpecifierIsSubclassOf(a, b);
        }

        public IEnumerable<MethodSpecifier> GetStaticFunctions()
        {
            return reflectionProvider.GetStaticFunctions().ToArray();
        }

        public IEnumerable<MethodSpecifier> GetStaticFunctionsWithReturnType(TypeSpecifier returnTypeSpecifier)
        {
            return reflectionProvider.GetStaticFunctionsWithReturnType(returnTypeSpecifier).ToArray();
        }

        public IEnumerable<TypeSpecifier> GetNonStaticTypes()
        {
            return reflectionProvider.GetNonStaticTypes().ToArray();
        }

        public IEnumerable<MethodSpecifier> GetPublicMethodsForType(TypeSpecifier typeSpecifier)
        {
            return reflectionProvider.GetPublicMethodsForType(typeSpecifier).ToArray();
        }

        public IEnumerable<PropertySpecifier> GetPublicPropertiesForType(TypeSpecifier typeSpecifier)
        {
            return reflectionProvider.GetPublicPropertiesForType(typeSpecifier).ToArray();
        }

        public IEnumerable<ConstructorSpecifier> GetConstructors(TypeSpecifier typeSpecifier)
        {
            return reflectionProvider.GetConstructors(typeSpecifier).ToArray();
        }

        public IEnumerable<string> GetEnumNames(TypeSpecifier typeSpecifier)
        {
            return reflectionProvider.GetEnumNames(typeSpecifier).ToArray();
        }

        public IEnumerable<MethodSpecifier> GetStaticFunctionsWithArgumentType(TypeSpecifier typeSpecifier)
        {
            return reflectionProvider.GetStaticFunctionsWithArgumentType(typeSpecifier).ToArray();
        }
        #endregion
    }
}
