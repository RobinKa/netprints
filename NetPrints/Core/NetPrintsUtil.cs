using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
