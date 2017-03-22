using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetPrintsEditor.Interop
{
    public abstract class WrappedAppDomainObject : MarshalByRefObject
    {
        public virtual void Initialize(IEnumerable<string> assemblyPaths)
        {
            foreach (string assemblyPath in assemblyPaths)
            {
                Assembly.LoadFrom(assemblyPath);
            }
        }
    }
}
