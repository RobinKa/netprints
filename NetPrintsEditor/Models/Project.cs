using Microsoft.CodeAnalysis;
using NetPrints.Core;
using NetPrintsEditor.Compilation;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace NetPrintsEditor.Models
{
    [DataContract]
    public class Project
    {
        private static readonly LocalAssemblyName[] DefaultAssemblies = new LocalAssemblyName[]
        {
            new LocalFrameworkAssemblyName("System", ".NETFramework/v4.0"),
            new LocalFrameworkAssemblyName("System.Core", ".NETFramework/v4.0"),
            new LocalFrameworkAssemblyName("mscorlib", ".NETFramework/v4.0"),
        };

        private static readonly DataContractSerializer ProjectSerializer = new DataContractSerializer(typeof(Project));

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

            if (addDefaultAssemblies)
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
