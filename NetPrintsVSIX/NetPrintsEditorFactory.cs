using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using NetPrints.Core;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace NetPrintsVSIX
{
    [ProvideEditorExtension(typeof(NetPrintsEditorFactory), ".netpp", 32)]
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

            // Reference these just because otherwise mah metro won't get included correctly.
            UserControl1 ctl = new UserControl1();

            MahApps.Metro.IconPacks.PackIconMaterial x = new MahApps.Metro.IconPacks.PackIconMaterial()
            {
                Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Minus
            };

            MahApps.Metro.Controls.MetroWindow y = null;

            // Load the project
            NetPrints.Core.Project project = NetPrints.Core.Project.LoadFromPath(pszMkDocument);

            package.ReplaceProjectReferences(project);

            // Setup the reflection provider
            {
                var references = project.References;

                // Add referenced assemblies
                var assemblyPaths = references.OfType<AssemblyReference>().Select(assemblyRef => assemblyRef.AssemblyPath);

                // Add source files
                var sourcePaths = references.OfType<SourceDirectoryReference>().SelectMany(directoryRef => directoryRef.SourceFilePaths);

                // Add our own sources
                var sources = project.GenerateClassSources();

                NetPrintsEditor.App.ReloadReflectionProvider(assemblyPaths, sourcePaths, sources);
            }

            var cls = project.Classes[0];

            // Create the class editor view
            var classWindow = new NetPrintsEditor.ClassEditorView()
            {
                DataContext = new NetPrintsEditor.ViewModels.ClassEditorVM(cls)
            };

            ppunkDocView = Marshal.GetIUnknownForObject(classWindow);
            ppunkDocData = Marshal.GetIUnknownForObject(classWindow);
            pbstrEditorCaption = "";

            return VSConstants.S_OK;
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
