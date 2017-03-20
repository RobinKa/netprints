using System;
using System.Collections.Generic;
using System.Reflection;

namespace NetPrints.Core
{
    public static class NetPrintsUtil
    {
        public static string GetUniqueName(string name, IList<string> names)
        {
            int i = 1;

            while (true)
            {
                string uniqueName = i == 1 ? name : $"{name}{i}";

                if (!names.Contains(uniqueName))
                {
                    return uniqueName;
                }

                i++;
            }
        }

        public static Type GetTypeFromFullName(string fullName)
        {
            foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = assembly.GetType(fullName);
                if (t != null)
                {
                    return t;
                }
            }

            return null;
        }
    }
}
