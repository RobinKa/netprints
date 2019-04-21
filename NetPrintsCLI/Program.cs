using CommandLine;
using NetPrints.Core;
using System;
using System.Threading;

namespace NetPrintsCLI
{
    class Program
    {
        public class CompileOptions
        {
            [Option('p', "project-path", Required = false, HelpText = "Path to the project file (.netpp).")]
            public string ProjectPath { get; set; }

            [Option('r', "run", Required = false, HelpText = "Whether to run the executable on success.")]
            public bool Run { get; set; }
        }

        private static int Compile(CompileOptions options)
        {
            Console.WriteLine("Compiling {0}", options.ProjectPath);

            Project project = Project.LoadFromPath(options.ProjectPath);
            project.CompileProject();

            while (project.IsCompiling)
            {
                Console.Write(".");
                Thread.Sleep(1000);
            }

            Console.WriteLine();

            if (project.LastCompilationSucceeded)
            {
                Console.WriteLine("Compilation succeeded.");

                if (options.Run)
                {
                    Console.WriteLine($"Running...");
                    project.RunProject();
                }

                return 1;
            }
            else
            {
                Console.WriteLine($"Compilation failed with {project.LastCompileErrors.Count} errors:");
                foreach (var error in project.LastCompileErrors)
                {
                    Console.WriteLine(error);
                }
            }

            return 0;
        }

        private static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<CompileOptions>(args)
                .MapResult(Compile, errors => 1);
        }
    }
}
