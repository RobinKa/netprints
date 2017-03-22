using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetPrintsEditor.Interop
{
    public class AppDomainObject<T> where T : MarshalByRefObject
    {
        public T Object { get; private set; }

        private AppDomain appDomain;

        public AppDomainObject(AppDomain domain, T obj)
        {
            appDomain = domain;
            Object = obj;
        }

        public void Unload()
        {
            Object = null;
            AppDomain.Unload(appDomain);
            GC.Collect();
        }
    }

    public static class AppDomainHelper
    {
        public static AppDomainObject<T> Create<T>() where T : MarshalByRefObject
        {
            // Set shadow copy to true so we can still compile while we are loaded
            // Might not be necessary if we unload the app domain before compilation

            AppDomainSetup domainSetup = new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                ShadowCopyFiles = true.ToString(),
            };

            AppDomain domain = AppDomain.CreateDomain(nameof(T), null, domainSetup);

            T domainObject = (T)domain.CreateInstanceFromAndUnwrap(
                    typeof(T).Assembly.Location, typeof(T).FullName);
            
            return new AppDomainObject<T>(domain, domainObject);
        }
    }
}
