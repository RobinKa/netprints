using System;
using System.Collections.Generic;
using System.Reflection;

namespace NetPrints.Core
{
    public static class NetPrintsUtil
    {
        /// <summary>
        /// Returns the first name not already contained in a list of names by
        /// trying to generate a unique name based on the given name.
        /// </summary>
        /// <param name="name">Name to make unique.</param>
        /// <param name="names">List of names already existing.</param>
        /// <returns>Name based on name but not contained in names.</returns>
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
    }
}
