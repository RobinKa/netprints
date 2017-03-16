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

        public static IEnumerable<Type> GetNonStaticTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
                a.GetTypes().Where(t => t.IsPublic && !(t.IsAbstract && t.IsSealed)));
        }
    }
}
