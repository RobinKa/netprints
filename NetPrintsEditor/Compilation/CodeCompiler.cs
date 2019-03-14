using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Collections.Generic;
using System.Linq;

namespace NetPrintsEditor.Compilation
{
    /// <summary>
    /// Compiles code into binaries.
    /// </summary>
    public class CodeCompiler : ICodeCompiler
    {
        /// <summary>
        /// Compiles code into a binary.
        /// </summary>
        /// <param name="outputPath">Output path for the compilation.</param>
        /// <param name="assemblyPaths">Paths to assemblies to reference.</param>
        /// <param name="sources">Source code to compile.</param>
        /// <param name="generateExecutable">Whether to generate an executable or a dynamically linked library.</param>
        /// <returns>Results for the compilation.</returns>
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

            return new CodeCompileResults(emitResult.Success, errors, emitResult.Success ? outputPath : null);
        }
    }
}
