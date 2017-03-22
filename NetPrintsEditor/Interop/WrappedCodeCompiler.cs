using NetPrintsEditor.Compilation;
using NetPrintsEditor.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetPrintsEditor.Interop
{
    public class WrappedCodeCompiler : MarshalByRefObject, ICodeCompiler
    {
        private ICodeCompiler codeCompiler;

        public void LoadRequiredAssemblies(IEnumerable<string> assemblyPaths)
        {
            foreach (string assemblyPath in assemblyPaths)
            {
                Assembly.LoadFrom(assemblyPath);
            }

            codeCompiler = new CodeCompiler();
        }

        public CodeCompileResults CompileSources(string outputPath, IEnumerable<string> assemblyPaths, 
            IEnumerable<string> sources, bool generateExecutable)
        {
            return codeCompiler.CompileSources(outputPath, assemblyPaths, sources, generateExecutable);
        }
    }
}
