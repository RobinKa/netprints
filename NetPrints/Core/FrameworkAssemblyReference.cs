using System;
using System.IO;
using System.Runtime.Serialization;

namespace NetPrints.Core
{
    [DataContract]
    public class FrameworkAssemblyReference : AssemblyReference
    {
        /// <summary>
        /// Path relative to reference assemblies path.
        /// </summary>
        public string FrameworkRelativePath
        {
            get => frameworkRelativePath;
            private set
            {
                frameworkRelativePath = value;
                UpdateFrameworkPath();
            }
        }

        [DataMember]
        private string frameworkRelativePath;

        public FrameworkAssemblyReference(string relativePath)
            : base(null)
        {
            FrameworkRelativePath = relativePath;
        }

        private void UpdateFrameworkPath()
        {
            AssemblyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Reference Assemblies", "Microsoft", "Framework", FrameworkRelativePath);
        }

        public override string ToString() =>
            $"Framework reference assembly {FrameworkRelativePath} found at {AssemblyPath}";
    }
}
