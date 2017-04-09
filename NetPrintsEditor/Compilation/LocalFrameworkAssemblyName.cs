using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace NetPrintsEditor.Compilation
{
    [DataContract]
    public class LocalFrameworkAssemblyName : LocalAssemblyName
    {
        [DataMember]
        public string FrameworkVersion
        {
            get;
            set;
        }

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

        public override Assembly LoadAssembly()
        {
            return Assembly.LoadFrom(Path);
        }

        public override bool FixPath()
        {
            Path = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Reference Assemblies/Microsoft/Framework/.NETFramework",
                FrameworkVersion,
                $"{FrameworkAssemblyName}.dll");

            Assembly assembly = Assembly.UnsafeLoadFrom(Path);
            Name = assembly.FullName;

            return true;
        }
    }
}
