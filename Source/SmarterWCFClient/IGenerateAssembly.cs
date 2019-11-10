using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

/*  References:
        https://stackoverflow.com/a/57468601  
*/

namespace SmarterWCFClient
{
    public interface IGenerateAssembly
    {
        Assembly GenerateAssemblyFrom(SyntaxTree syntaxTree, params Assembly[] additionalReferences);
    }

    public class AssemblyGenerator : IGenerateAssembly
    {
        private string _CoreAssemblyFolder = @"C:\Program Files\dotnet\sdk\NuGetFallbackFolder\microsoft.netcore.app\2.1.0\ref\netcoreapp2.1";
        private static string _FrameworkAssemblyFolder = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8";

        private static IEnumerable<PortableExecutableReference> CreateNetCoreReferences()
        {
            foreach (var dllFile in Directory.GetFiles(@"C:\Program Files\dotnet\sdk\NuGetFallbackFolder\microsoft.netcore.app\2.2.0\ref\netcoreapp2.2", "*.dll"))
            {
                yield return MetadataReference.CreateFromFile(dllFile);
            }
        }

        private static IEnumerable<PortableExecutableReference> CreateNetFullReferences()
        {
            var dir = _FrameworkAssemblyFolder;
            var assemblies = Directory.GetFiles(dir, "*.dll").ToList();
            var filteredAssemblies = FilterInvalidAssembies(assemblies);

            foreach (var assembly in filteredAssemblies)
            {
                yield return MetadataReference.CreateFromFile(assembly);
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

        private static List<string> FilterInvalidAssembies(List<string> files)
        {
            RemoveStringFromList(files, "System.EnterpriseServices.Wrapper.dll");
            RemoveStringFromList(files, "System.EnterpriseServices.Thunk.dll");
            
            return files;
        }

        private static void RemoveStringFromList(List<string> items, string contains)
        {
            var item = items.Where(q => q.Contains(contains)).Select(q => q).FirstOrDefault();
            if (item != null)
            {
                items.Remove(item);
            }
        }

        public Assembly GenerateAssemblyFrom(SyntaxTree syntaxTree, params Assembly[] additionalReferences)
        {
            var references = new List<PortableExecutableReference>(CreateNetFullReferences());

            string assemblyName = Path.GetRandomFileName();
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            references.AddRange(additionalReferences.Select(x => MetadataReference.CreateFromFile(x.Location)).ToList());
            references.Add(MetadataReference.CreateFromFile(typeof(IGenerateCodeForChannelClient).Assembly.Location));

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