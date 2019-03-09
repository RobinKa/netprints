using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetPrintsEditor.Compilation
{
    public class CodeCompiler : ICodeCompiler
    {
        public CodeCompileResults CompileSources(string outputPath, IEnumerable<string> assemblyPaths,
            IEnumerable<string> sources, bool generateExecutable)
        {
            IEnumerable<SyntaxTree> syntaxTrees = sources.Select(source => SyntaxFactory.ParseSyntaxTree(source));
            IEnumerable<MetadataReference> references = assemblyPaths.Select(path => MetadataReference.CreateFromFile(path));
            var compilationOptions = new CSharpCompilationOptions(generateExecutable ? OutputKind.ConsoleApplication : OutputKind.DynamicallyLinkedLibrary);

            CSharpCompilation compilation = CSharpCompilation.Create("NetPrintsOutput")
                .WithOptions(compilationOptions)
                .AddReferences(references)
                .AddSyntaxTrees(syntaxTrees);

            var emitResult = compilation.Emit(outputPath);

            return new CodeCompileResults(emitResult.Success, new List<string>(), outputPath);
        }
    }
}
