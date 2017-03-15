using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace NetPrintsEditor
{
    public static class ReflectionUtil
    {
        public static IEnumerable<MethodInfo> GetStaticFunctions()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
                a.GetTypes().Where(t => t.IsPublic).SelectMany(t =>
                    t.GetMethods(BindingFlags.Static | BindingFlags.Public)
            ));
        }
    }
}
