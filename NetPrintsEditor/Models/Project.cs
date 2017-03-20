using NetPrints.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetPrintsEditor.Models
{
    [DataContract]
    public class Project
    {
        private static readonly string[] DefaultAssemblies = new string[]
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll",
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
        public ObservableCollection<string> ClassPaths
        {
            get;
            set;
        } = new ObservableCollection<string>();

        [DataMember]
        public ObservableCollection<string> Assemblies
        {
            get;
            set;
        } = new ObservableCollection<string>();

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
