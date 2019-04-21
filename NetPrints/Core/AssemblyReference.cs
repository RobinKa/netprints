using System.IO;
using System.Runtime.Serialization;

namespace NetPrints.Core
{
    [DataContract]
    public class AssemblyReference : CompilationReference
    {
        [DataMember]
        public string AssemblyPath
        {
            get;
            set;
        }

        public AssemblyReference(string assemblyPath)
        {
            AssemblyPath = assemblyPath;
        }

        public override string ToString() => $"{Path.GetFileNameWithoutExtension(AssemblyPath)} at {AssemblyPath}";
    }
}
