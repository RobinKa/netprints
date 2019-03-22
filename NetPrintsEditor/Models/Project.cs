using NetPrints.Core;
using NetPrintsEditor.Compilation;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;

namespace NetPrintsEditor.Models
{
    [Flags]
    public enum ProjectCompilationOutput
    {
        Nothing = 0,
        SourceCode = 1,
        Binaries = 2,
        Errors = 4,
        All = SourceCode | Binaries | Errors,
    }

    /// <summary>
    /// Project model.
    /// </summary>
    [DataContract]
    public class Project
    {
        private static readonly LocalAssemblyName[] DefaultAssemblies = new LocalAssemblyName[]
        {
            new LocalFrameworkAssemblyName("System", ".NETFramework/v4.5"),
            new LocalFrameworkAssemblyName("System.Core", ".NETFramework/v4.5"),
            new LocalFrameworkAssemblyName("System.Collections", ".NETFramework/v4.5/Facades"),
            new LocalFrameworkAssemblyName("System.IO", ".NETFramework/v4.5/Facades"),
            new LocalFrameworkAssemblyName("System.Linq", ".NETFramework/v4.5/Facades"),
            new LocalFrameworkAssemblyName("System.Threading", ".NETFramework/v4.5/Facades"),
            new LocalFrameworkAssemblyName("mscorlib", ".NETFramework/v4.5"),
        };

        private static readonly DataContractSerializer ProjectSerializer = new DataContractSerializer(typeof(Project));

        /// <summary>
        /// Name of the project.
        /// </summary>
        [DataMember]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Version of the editor that the project was saved in.
        /// </summary>
        [DataMember]
        public Version SaveVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Path to the last successfully compiled assembly.
        /// </summary>
        [DataMember]
        public string LastCompiledAssemblyPath
        {
            get;
            set;
        }

        /// <summary>
        /// Path to the project file.
        /// </summary>
        public string Path
        {
            get;
            set;
        }

        /// <summary>
        /// Default namespace of newly created classes.
        /// </summary>
        [DataMember]
        public string DefaultNamespace
        {
            get;
            set;
        }

        /// <summary>
        /// Paths to files for the class models within this project.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<string> ClassPaths
        {
            get;
            set;
        } = new ObservableRangeCollection<string>();

        /// <summary>
        /// Assemblies referenced by this project.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<LocalAssemblyName> Assemblies
        {
            get;
            set;
        } = new ObservableRangeCollection<LocalAssemblyName>();

        /// <summary>
        /// Determines what gets output during compilation.
        /// </summary>
        [DataMember]
        public ProjectCompilationOutput CompilationOutput
        {
            get;
            set;
        }

        private Project()
        {

        }

        /// <summary>
        /// Saves the project to its path.
        /// </summary>
        public void Save()
        {
            SaveVersion = Assembly.GetExecutingAssembly().GetName().Version;

            using (FileStream fileStream = File.Open(Path, FileMode.Create))
            {
                ProjectSerializer.WriteObject(fileStream, this);
            }
        }

        /// <summary>
        /// Creates a new project.
        /// </summary>
        /// <param name="name">Name of the project.</param>
        /// <param name="defaultNamespace">Default namespace of the project.</param>
        /// <param name="addDefaultAssemblies">Whether to add default assemblies to the project.</param>
        /// <returns>The created project.</returns>
        public static Project CreateNew(string name, string defaultNamespace, bool addDefaultAssemblies=true,
            ProjectCompilationOutput compilationOutput=ProjectCompilationOutput.All)
        {
            Project project = new Project()
            {
                Name = name,
                DefaultNamespace = defaultNamespace,
                CompilationOutput = compilationOutput
            };

            if (addDefaultAssemblies)
            {
                project.Assemblies.AddRange(DefaultAssemblies);
            }

            return project;
        }

        /// <summary>
        /// Loads a project from a path.
        /// </summary>
        /// <param name="path">Path to the project file.</param>
        /// <returns>Loaded project or null if unsuccessful</returns>
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
