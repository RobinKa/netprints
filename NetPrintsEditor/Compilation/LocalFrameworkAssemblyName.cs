using System;
using System.Runtime.Serialization;

namespace NetPrintsEditor.Compilation
{
    /// <summary>
    /// Metadata for framework assemblies.
    /// </summary>
    [DataContract]
    [Obsolete]
    public class LocalFrameworkAssemblyName : LocalAssemblyName
    {
        /// <summary>
        /// Version of the framework.
        /// </summary>
        [DataMember]
        public string FrameworkVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Assembly name.
        /// </summary>
        [DataMember]
        public string FrameworkAssemblyName
        {
            get;
            set;
        }

        public LocalFrameworkAssemblyName(string frameworkAssemblyName, string frameworkVersion)
            : base(null, null)
        {
            FrameworkAssemblyName = frameworkAssemblyName;
            FrameworkVersion = frameworkVersion;

            if (!FixPath())
            {
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// Tries to fix an assembly path.
        /// </summary>
        /// <returns>Whether the assembly path is now valid.</returns>
        public override bool FixPath()
        {
            Path = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Reference Assemblies/Microsoft/Framework/",
                FrameworkVersion,
                $"{FrameworkAssemblyName}.dll");

            Name = System.IO.Path.GetFileNameWithoutExtension(Path);

            return System.IO.File.Exists(Path);
        }
    }
}
