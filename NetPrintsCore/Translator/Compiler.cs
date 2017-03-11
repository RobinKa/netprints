using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Runtime.Loader;
using System.Reflection;
using Microsoft.CodeAnalysis.Emit;

namespace NetPrintsCore.Translator
{
    static class Compiler
    {
        public static bool CompileDLL(string text, string outputPath)
        {
            CSharpCompilation compilation = CSharpCompilation.Create("a")
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location))
                .AddSyntaxTrees(CSharpSyntaxTree.ParseText(text));

            EmitResult result = compilation.Emit(outputPath);

            return result.Success;
        }
    }
}
