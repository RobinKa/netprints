using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using NetPrints.Core;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace NetPrints.VSIX
{
    [ProvideEditorExtension(typeof(NetPrintsEditorFactory), ".netpc", 32)]
    public class NetPrintsEditorFactory : IVsEditorFactory, IDisposable
    {
        private const string BinaryPathEnvVar = "NETPRINTSEDITOR";
        private ServiceProvider vsServiceProvider;
        private readonly NetPrintsVSIXPackage package;

        public NetPrintsEditorFactory(NetPrintsVSIXPackage pkg)
        {
            package = pkg;
        }

        [EnvironmentPermission(SecurityAction.Demand, Unrestricted = true)]
        public int CreateEditorInstance(uint grfCreateDoc, string pszMkDocument, string pszPhysicalView, IVsHierarchy pvHier,
                                        uint itemid, IntPtr punkDocDataExisting, out IntPtr ppunkDocView, out IntPtr ppunkDocData,
                                        out string pbstrEditorCaption, out Guid pguidCmdUI, out int pgrfCDW)
        {
            // Initialize to null
            ppunkDocView = IntPtr.Zero;
            ppunkDocData = IntPtr.Zero;
            pguidCmdUI = new Guid("{0367BD5A-5B23-41D1-A07B-69312569B997}");
            pgrfCDW = 0;
            pbstrEditorCaption = null;

            // Validate inputs
            if ((grfCreateDoc & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
            {
                return VSConstants.E_INVALIDARG;
            }
            if (punkDocDataExisting != IntPtr.Zero)
            {
                return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
            }

            // Load the class
            var cls = NetPrints.Serialization.SerializationHelper.LoadClass(pszMkDocument);

            ReloadReflection();

            // Create the class editor view
            var classWindow = new ClassEditorView()
            {
                DataContext = new NetPrintsEditor.ViewModels.ClassEditorVM(cls),
                ReloadReflectionProvider = ReloadReflection
            };

            ppunkDocView = Marshal.GetIUnknownForObject(classWindow);
            ppunkDocData = Marshal.GetIUnknownForObject(classWindow);
            pbstrEditorCaption = "";

            return VSConstants.S_OK;
        }

        private void ReloadReflection()
        {
            // Add referenced assemblies
            var assemblyPaths = package.GetAssemblyReferences().Select(assemblyRef => assemblyRef.AssemblyPath);

            // Get source files in projects
            var sourcePaths = package.GetSourceDirectoryReferences().SelectMany(sourceRef => sourceRef.SourceFilePaths);

            // Add our own sources
            var sources = package.GetGeneratedCode();

            NetPrintsEditor.App.ReloadReflectionProvider(assemblyPaths, sourcePaths, sources);
        }

        public int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp)
        {
            vsServiceProvider = new ServiceProvider(psp);
            return VSConstants.S_OK;
        }

        public int Close() => VSConstants.S_OK;

        public int MapLogicalView(ref Guid rguidLogicalView, out string pbstrPhysicalView)
        {
            pbstrPhysicalView = null;   // initialize out parameter

            // we support only a single physical view
            if (VSConstants.LOGVIEWID_Primary == rguidLogicalView)
            {
                // primary view uses NULL as pbstrPhysicalView
                return VSConstants.S_OK;
            }
            else
            {
                // you must return E_NOTIMPL for any unrecognized rguidLogicalView values
                return VSConstants.E_NOTIMPL;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // If disposing equals true, dispose all managed and unmanaged resources
            if (disposing)
            {
                /// Since we create a ServiceProvider which implements IDisposable we
                /// also need to implement IDisposable to make sure that the ServiceProvider's
                /// Dispose method gets called.
                if (vsServiceProvider != null)
                {
                    vsServiceProvider.Dispose();
                    vsServiceProvider = null;
                }
            }
        }
    }
}
