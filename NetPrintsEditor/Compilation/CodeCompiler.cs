using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrintsEditor.Compilation
{
    public class CodeCompiler : ICodeCompiler
    {
        public CodeCompileResults CompileSources(string outputPath, IEnumerable<string> assemblyPaths,
            IEnumerable<string> sources, bool generateExecutable)
        {
            CSharpCodeProvider csc = new CSharpCodeProvider();

            CompilerParameters parameters = new CompilerParameters(assemblyPaths.ToArray(), outputPath, true)
            {
                GenerateExecutable = generateExecutable,
                CompilerOptions = generateExecutable ? "/platform:anycpu32bitpreferred" : null
            };

            CompilerResults results = csc.CompileAssemblyFromSource(parameters, sources.ToArray());

            IEnumerable<string> errors = results.Errors.Cast<CompilerError>().Select(err => err.ToString()).ToArray();

            CodeCompileResults codeCompileResults = new CodeCompileResults(
                !results.Errors.HasErrors, errors, results.PathToAssembly);

            return codeCompileResults;
        }
    }
}
