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
    public class WrappedCodeCompiler : WrappedAppDomainObject, ICodeCompiler
    {
        private ICodeCompiler codeCompiler;

        public override void Initialize(IEnumerable<string> assemblyPaths)
        {
            base.Initialize(assemblyPaths);

            codeCompiler = new CodeCompiler();
        }

        public CodeCompileResults CompileSources(string outputPath, IEnumerable<string> assemblyPaths, 
            IEnumerable<string> sources, bool generateExecutable)
        {
            return codeCompiler.CompileSources(outputPath, assemblyPaths, sources, generateExecutable);
        }
    }
}
