using NetPrints.Core;
using NetPrintsEditor.Compilation;
using System.IO;
using System.Runtime.Serialization;

namespace NetPrintsEditor.Models
{
    [DataContract]
    public class Project
    {
        private static readonly LocalAssemblyName[] DefaultAssemblies = new LocalAssemblyName[]
        {
            LocalAssemblyName.FromName("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
            LocalAssemblyName.FromName("System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
            LocalAssemblyName.FromName("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
        };

        private static readonly DataContractSerializer ProjectSerializer = new DataContractSerializer(
                typeof(Project), null, int.MaxValue, false, true, null);

        [DataMember]
        public string Name
        {
            get;
            set;
        }

        [DataMember]
        public string LastCompiledAssemblyPath
        {
            get;
            set;
        }

        public string Path
        {
            get;
            set;
        }

        [DataMember]
        public string DefaultNamespace
        {
            get;
            set;
        }

        [DataMember]
        public ObservableRangeCollection<string> ClassPaths
        {
            get;
            set;
        } = new ObservableRangeCollection<string>();

        [DataMember]
        public ObservableRangeCollection<LocalAssemblyName> Assemblies
        {
            get;
            set;
        } = new ObservableRangeCollection<LocalAssemblyName>();

        private Project()
        {

        }

        public void Save()
        {
            using (FileStream fileStream = File.Open(Path, FileMode.Create))
            {
                ProjectSerializer.WriteObject(fileStream, this);
            }
        }

        public static Project CreateNew(string name, string defaultNamespace, bool addDefaultAssemblies=true)
        {
            Project project = new Project()
            {
                Name = name,
                DefaultNamespace = defaultNamespace
            };

            if(addDefaultAssemblies)
            {
                project.Assemblies.AddRange(DefaultAssemblies);
            }

            return project;
        }

        public static Project LoadFromPath(string path)
        {
            using (FileStream fileStream = File.OpenRead(path))
            {
                if (ProjectSerializer.ReadObject(fileStream) is Project project)
                {
                    project.Path = path;
                    return project;
                }
            }

            return null;
        }
    }
}
