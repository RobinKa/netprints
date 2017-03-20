using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace NetPrintsEditor.Compilation
{
    [DataContract]
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
            Assembly assembly = Assembly.UnsafeLoadFrom(path);
            return new LocalAssemblyName(assembly.FullName, assembly.Location);
        }

        public static LocalAssemblyName FromName(string name)
        {
            Assembly assembly = Assembly.Load(name);
            return new LocalAssemblyName(assembly.FullName, assembly.Location);
        }

        public Assembly LoadAssembly()
        {
            // First try to load from name, then from path
            // Make sure the name is correct when loading from path

            if(!FixPath())
            {
                return null;
            }

            try
            {
                return Assembly.Load(Name);
            }
            catch
            {
                Assembly loadedAssembly = Assembly.UnsafeLoadFrom(Path);

                if(loadedAssembly.FullName != Name)
                {
                    throw new Exception("Loaded assembly name doesnt equal name");
                }

                return loadedAssembly;
            }
        }

        public bool FixPath()
        {
            try
            {
                // Check if assembly name at path matches Name
                if (Assembly.UnsafeLoadFrom(Path).FullName == Name)
                {
                    return true;
                }
            }
            catch
            {
                try
                {
                    // Try to get path from assembly name
                    Path = Assembly.Load(Name).Location ?? throw new Exception();
                    return true;
                }
                catch
                {
                    // We couldnt get the name from the path nor the path from the name
                    return false;
                }
            }

            throw new Exception("This should never happen");
        }
    }
}
