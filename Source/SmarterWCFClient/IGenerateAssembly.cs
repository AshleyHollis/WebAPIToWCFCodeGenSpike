using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
//using SmartHotel.Registration.Wcf;
//using SmartHotel.Registration.Wcf.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SmarterWCFClient
{
    public interface IGenerateAssembly
    {
        Assembly GenerateAssemblyFrom(SyntaxTree syntaxTree, params Assembly[] additionalReferences);
    }

    public class AssemblyGenerator : IGenerateAssembly
    {
        private static IEnumerable<PortableExecutableReference> CreateNetCoreReferences()
        {
            foreach (var dllFile in Directory.GetFiles(@"C:\Program Files\dotnet\sdk\NuGetFallbackFolder\microsoft.netcore.app\2.2.0\ref\netcoreapp2.2", "*.dll"))
            {
                yield return MetadataReference.CreateFromFile(dllFile);
            }
        }

        private static readonly IEnumerable<string> DefaultNamespaces = new[]
      {
            "System",
            "System.IO",
            "System.Net",
            "System.Linq",
            "System.Text",
            "System.Text.RegularExpressions",
            "System.Collections.Generic"
        };

        public Assembly GenerateAssemblyFrom(SyntaxTree syntaxTree, params Assembly[] additionalReferences)
        {
            string assemblyName = Path.GetRandomFileName();
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            var references = additionalReferences.Select(x => MetadataReference.CreateFromFile(x.Location)).ToList();
            references.Add(MetadataReference.CreateFromFile(typeof(IGenerateCodeForChannelClient).Assembly.Location));
            
            //references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            //references.Add(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));

            //references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")));
            //references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")));
            //references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")));
            //references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));

            //references.Add(MetadataReference.CreateFromFile(typeof(IService).Assembly.Location));
            //references.Add(MetadataReference.CreateFromFile(typeof(Registration).Assembly.Location));
            //references.Add(MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location));
            references.AddRange(CreateNetCoreReferences());

            CSharpCompilationOptions defaultCompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                   .WithOverflowChecks(true)
                   .WithOptimizationLevel(OptimizationLevel.Release)
                   .WithUsings(DefaultNamespaces);

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: defaultCompilationOptions);

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly assembly = Assembly.Load(ms.ToArray());
                    return assembly;
                }
            }
            return null;
        }
    }
}