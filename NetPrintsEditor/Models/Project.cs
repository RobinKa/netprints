using NetPrints.Core;
using System.IO;
using System.Runtime.Serialization;

namespace NetPrintsEditor.Models
{
    [DataContract]
    public class Project
    {
        private static readonly string[] DefaultAssemblies = new string[]
        {
            "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
            "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
            "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
        };

        private static readonly DataContractSerializer ProjectSerializer = new DataContractSerializer(
                typeof(Project), null, int.MaxValue, false, true, null);

        [DataMember]
        public string Name
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
        public ObservableRangeCollection<string> Assemblies
        {
            get;
            set;
        } = new ObservableRangeCollection<string>();

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
