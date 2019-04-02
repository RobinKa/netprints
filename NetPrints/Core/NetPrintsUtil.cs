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

        /// <summary>
        /// Returns whether a given visibility for a member of a type is visible from another type.
        /// </summary>
        /// <param name="fromType">Type that we are seeing from.</param>
        /// <param name="type">Type that we are looking at.</param>
        /// <param name="visibility">Visibility of the member on type.</param>
        /// <returns></returns>
        public static bool IsVisible(TypeSpecifier fromType, TypeSpecifier type, MemberVisibility visibility, Func<TypeSpecifier, TypeSpecifier, bool> isSubclassOf)
        {
            // TODO: Internal

            if (fromType == type)
            {
                return true;
            }
            else if (isSubclassOf(fromType, type))
            {
                return visibility.HasFlag(MemberVisibility.ProtectedOrPublic);
            }

            return visibility.HasFlag(MemberVisibility.Public);
        }
    }
}
