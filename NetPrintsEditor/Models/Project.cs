using NetPrints.Core;
using NetPrintsEditor.Compilation;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

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

    public enum BinaryType
    {
        SharedLibrary,
        Executable,
    }

    /// <summary>
    /// Project model.
    /// </summary>
    [DataContract]
    public class Project
    {
        private static readonly IEnumerable<FrameworkAssemblyReference> DefaultReferences = new FrameworkAssemblyReference[]
        {
            new FrameworkAssemblyReference(".NETFramework/v4.5/System.dll"),
            new FrameworkAssemblyReference(".NETFramework/v4.5/System.Core.dll"),
            new FrameworkAssemblyReference(".NETFramework/v4.5/mscorlib.dll"),
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
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [Obsolete("Use References instead.")]
        public ObservableRangeCollection<LocalAssemblyName> Assemblies
        {
            get => null;
            set
            {
                if (value != null)
                {
                    References = new ObservableRangeCollection<CompilationReference>();

                    var frameworkAssemblyNames = value.OfType<LocalFrameworkAssemblyName>().ToList();

                    frameworkAssemblyNames.ForEach(a => References.Add(new FrameworkAssemblyReference($"{a.FrameworkVersion}/{a.FrameworkAssemblyName}.dll")));
                    value.Except(frameworkAssemblyNames).ToList().ForEach(a => References.Add(new AssemblyReference(a.Path)));
                }
            }
        }

        /// <summary>
        /// References of this project.
        /// </summary>
        [DataMember]
        public ObservableRangeCollection<CompilationReference> References
        {
            get;
            set;
        } = new ObservableRangeCollection<CompilationReference>();

        /// <summary>
        /// Determines what gets output during compilation.
        /// </summary>
        [DataMember]
        public ProjectCompilationOutput CompilationOutput
        {
            get;
            set;
        }

        /// <summary>
        /// Type of the binary that we want to output.
        /// </summary>
        [DataMember]
        public BinaryType OutputBinaryType
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

            using FileStream fileStream = File.Open(Path, FileMode.Create);
            ProjectSerializer.WriteObject(fileStream, this);
        }

        /// <summary>
        /// Creates a new project.
        /// </summary>
        /// <param name="name">Name of the project.</param>
        /// <param name="defaultNamespace">Default namespace of the project.</param>
        /// <param name="addDefaultReferences">Whether to add default references to the project.</param>
        /// <returns>The created project.</returns>
        public static Project CreateNew(string name, string defaultNamespace, bool addDefaultReferences=true,
            ProjectCompilationOutput compilationOutput=ProjectCompilationOutput.All)
        {
            Project project = new Project()
            {
                Name = name,
                DefaultNamespace = defaultNamespace,
                CompilationOutput = compilationOutput
            };

            if (addDefaultReferences)
            {
                project.References.AddRange(DefaultReferences);
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
