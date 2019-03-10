using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Collections.Generic;
using System.Linq;

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

            EmitResult emitResult = compilation.Emit(outputPath);

            IEnumerable<string> errors = emitResult.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.GetMessage());

            return new CodeCompileResults(emitResult.Success, errors, outputPath);
        }
    }
}
