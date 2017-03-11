using System;
using System.Collections.Generic;
using System.Text;

namespace NetPrints.Translator
{
    public static class TranslatorUtil
    {
        public const string NAME_PREFIX = "var";

        public static string GetUniqueName(string name, IList<string> names)
        {
            int i = 1;

            while(true)
            {
                if(!names.Contains(name))
                {
                    return i == 1 ? $"{NAME_PREFIX}{name}" : $"{NAME_PREFIX}{name}{i}";
                }

                i++;
            }
        }
    }
}
