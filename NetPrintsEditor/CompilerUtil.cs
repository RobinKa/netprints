using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrintsEditor
{
    class CompilerUtil
    {
        public static CompilerResults CompileSources(string outputPath, 
            IEnumerable<string> assemblies, IEnumerable<string> sources, bool generateExecutable=false)
        {
            CSharpCodeProvider csc = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters(assemblies.ToArray(), outputPath, true);
            parameters.GenerateExecutable = generateExecutable;
            CompilerResults results = csc.CompileAssemblyFromSource(parameters, sources.ToArray());
            return results;
        }

        public static CompilerResults CompileStringToLibrary(string sourceCode, string outputPath)
        {
            CSharpCodeProvider csc = new CSharpCodeProvider();

            CompilerParameters parameters = new CompilerParameters(new string[] 
            {
                "mscorlib.dll",
                "System.dll",
                "System.Core.dll",
            }, outputPath, true);
            
            CompilerResults results = csc.CompileAssemblyFromSource(parameters, sourceCode);

            return results;
        }

        public static CompilerResults CompileStringToExecutable(string sourceCode, string outputPath)
        {
            CSharpCodeProvider csc = new CSharpCodeProvider();

            CompilerParameters parameters = new CompilerParameters(new string[]
            {
                "mscorlib.dll",
                "System.dll",
                "System.Core.dll",
            }, outputPath, true);

            parameters.GenerateExecutable = true;

            CompilerResults results = csc.CompileAssemblyFromSource(parameters, sourceCode);

            return results;
        }
    }
}
