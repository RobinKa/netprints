using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace NetPrintsEditor.Compilation
{
    [DataContract]
    [KnownType(typeof(LocalFrameworkAssemblyName))]
    public class LocalAssemblyName
    {
        [DataMember]
        public string Name
        {
            get;
            set;
        }

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

        public static LocalAssemblyName FromPath(string path)
        {
            return new LocalAssemblyName(System.IO.Path.GetFileNameWithoutExtension(path), path);
        }

        public static LocalAssemblyName FromName(string name)
        {
            Assembly assembly = Assembly.Load(name);
            return new LocalAssemblyName(assembly.FullName, assembly.Location);
        }

        public virtual bool FixPath()
        {
            return System.IO.File.Exists(Path);
        }
    }
}
