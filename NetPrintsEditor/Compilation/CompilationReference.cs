using System.Runtime.Serialization;

namespace NetPrintsEditor.Compilation
{
    [DataContract]
    [KnownType(typeof(AssemblyReference))]
    [KnownType(typeof(FrameworkAssemblyReference))]
    [KnownType(typeof(SourceDirectoryReference))]
    public abstract class CompilationReference : ICompilationReference
    {

    }
}
