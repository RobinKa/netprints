using System.Runtime.Serialization;

namespace NetPrintsEditor.Compilation
{
    /// <summary>
    /// Metadata for an assembly.
    /// </summary>
    [DataContract]
    [KnownType(typeof(LocalFrameworkAssemblyName))]
    public class LocalAssemblyName
    {
        /// <summary>
        /// Name of the assembly.
        /// </summary>
        [DataMember]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Path to the assembly.
        /// </summary>
        [DataMember]
        public string Path
        {
            get;
            set;
        }

        public LocalAssemblyName(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public override string ToString()
        {
            return $"{Name} at {Path}";
        }

        /// <summary>
        /// Creates a LocalAssemblyName from a path.
        /// </summary>
        /// <param name="path">Path to the assembly.</param>
        /// <returns>LocalAssemblyName for the given path.</returns>
        public static LocalAssemblyName FromPath(string path)
        {
            return new LocalAssemblyName(System.IO.Path.GetFileNameWithoutExtension(path), path);
        }

        /// <summary>
        /// Tries to fix an assembly path.
        /// </summary>
        /// <returns>Whether the assembly path is now valid.</returns>
        public virtual bool FixPath()
        {
            return System.IO.File.Exists(Path);
        }
    }
}
