using core.customers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using si.birokrat.next.common.build;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace gui_generator.cs_definition_serializer
{
    public class RuntimeCompilation
    {

        string assembly_output_file;
        IEnumerable<MetadataReference> assemblyReferences;
        public RuntimeCompilation(IEnumerable<MetadataReference> assemblyReferences, string assembly_output_file)
        {
            this.assembly_output_file = assembly_output_file;
            this.assemblyReferences = assemblyReferences;
        }

        public string CreateAssembly_Then_ReturnPath(string source)
        {
            var parsedSyntaxTree = Parse(source, "", CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp8));

            var refs = DefaultReferences.ToList();
            refs.AddRange(assemblyReferences);

            var compilation
                = CSharpCompilation.Create("Test.dll", new SyntaxTree[] { parsedSyntaxTree }, refs, DefaultCompilationOptions);
            try
            {
                var result = compilation.Emit(assembly_output_file);

                if (!result.Success)
                {
                    var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

                    // Convert the collection of Diagnostics into a readable string format
                    var errorMessage = failures
                        .Select(diagnostic => $"{diagnostic.Id}: {diagnostic.GetMessage()} (Line {diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1})")
                        .Aggregate((current, next) => current + "\n" + next);

                    throw new Exception($"Compilation failed!!!\nErrors:\n{errorMessage}");

                }
                return assembly_output_file;
            }
            catch (Exception ex)
            {
                throw new Exception("Compilation failed!!!");
            }

        }

        private static readonly IEnumerable<string> DefaultNamespaces =
            new[]
            {
                "System",
                "System.IO",
                "System.Net",
                "System.Linq",
                "System.Text",
                "System.Text.RegularExpressions",
                "System.Collections.Generic"
            };

        private IEnumerable<MetadataReference> DefaultReferences
        {
            get
            {
                var origi = new List<MetadataReference>
                    {
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Regex).Assembly.Location),
                    };
                return origi;
            }
        }

        private static readonly CSharpCompilationOptions DefaultCompilationOptions =
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithOverflowChecks(true).WithOptimizationLevel(OptimizationLevel.Release)
                    .WithUsings(DefaultNamespaces);

        private static SyntaxTree Parse(string text, string filename = "", CSharpParseOptions options = null)
        {
            var stringText = SourceText.From(text, Encoding.UTF8);
            return SyntaxFactory.ParseSyntaxTree(stringText, options, filename);
        }
    }
}
