using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.ObjectModel;

namespace NetPrintsEditor
{
    public static class ReflectionUtil
    {
        public static ObservableCollection<Type> NonStaticTypes = new ObservableCollection<Type>(GetNonStaticTypes());

        public static IEnumerable<MethodInfo> GetStaticFunctions()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
                a.GetTypes().Where(t => t.IsPublic).SelectMany(t =>
                    t.GetMethods(BindingFlags.Static | BindingFlags.Public)
            ));
        }

        public static IEnumerable<MethodInfo> GetStaticFunctionsWithReturnType(Type returnType)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
                a.GetTypes().Where(t => t.IsPublic).SelectMany(t =>
                    t.GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .Where(m => m.ReturnType == returnType)
            ));
        }

        public static IEnumerable<Type> GetNonStaticTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
                a.GetTypes().Where(t => t.IsPublic && !(t.IsAbstract && t.IsSealed)));
        }

        public static IEnumerable<MethodInfo> GetPublicMethodsForType(Type t)
        {
            return t.GetMethods(BindingFlags.Instance | BindingFlags.Public);
        }
    }
}
