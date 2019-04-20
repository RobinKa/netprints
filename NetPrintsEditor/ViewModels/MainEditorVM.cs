using GalaSoft.MvvmLight;
using NetPrints.Core;
using System.Linq;

namespace NetPrintsEditor.ViewModels
{
    public class MainEditorVM : ViewModelBase
    {
        public bool IsProjectOpen => Project != null;

        public bool CanCompile => Project?.CanCompile ?? false;

        public bool CanCompileAndRun => Project?.CanCompileAndRun ?? false;

        public Project Project
        {
            get; set;
        }

        public MainEditorVM(Project project)
        {
            Project = project;
            if (Project != null)
            {
                Project.References.CollectionChanged += (sender, e) => ReloadReflectionProvider();
            }
        }

        public void OnProjectChanged()
        {
            ReloadReflectionProvider();
        }

        private void ReloadReflectionProvider()
        {
            if (Project != null)
            {
                var references = Project.References;

                // Add referenced assemblies
                var assemblyPaths = references.OfType<AssemblyReference>().Select(assemblyRef => assemblyRef.AssemblyPath);

                // Add source files
                var sourcePaths = references.OfType<SourceDirectoryReference>().SelectMany(directoryRef => directoryRef.SourceFilePaths);

                // Add our own sources
                var sources = Project.GenerateClassSources();

                App.ReloadReflectionProvider(assemblyPaths, sourcePaths, sources);
            }
        }
    }
}
