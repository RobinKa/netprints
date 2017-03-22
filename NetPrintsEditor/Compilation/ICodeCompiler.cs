using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrintsEditor.Compilation
{
    [Serializable]
    public class CodeCompileResults
    {
        public bool Success
        {
            get;
        }

        public IEnumerable<string> Errors
        {
            get;
        }

        public string PathToAssembly
        {
            get;
        }

        public CodeCompileResults(bool success, IEnumerable<string> errors, string pathToAssembly)
        {
            Success = success;
            Errors = errors;
            PathToAssembly = pathToAssembly;
        }
    }

    public interface ICodeCompiler
    {
        CodeCompileResults CompileSources(string outputPath, IEnumerable<string> assemblyPaths,
            IEnumerable<string> sources, bool generateExecutable);
    }
}
