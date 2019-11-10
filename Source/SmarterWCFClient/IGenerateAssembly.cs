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
using System.Runtime.InteropServices;

namespace SmarterWCFClient
{
    public interface IGenerateAssembly
    {
        Assembly GenerateAssemblyFrom(SyntaxTree syntaxTree, params Assembly[] additionalReferences);
    }

    public class AssemblyGenerator : IGenerateAssembly
    {
        private string _CoreAssemblyFolder = @"C:\Program Files\dotnet\sdk\NuGetFallbackFolder\microsoft.netcore.app\2.1.0\ref\netcoreapp2.1";
        private string _FrameworkAssemblyFolder = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8";


        private static IEnumerable<PortableExecutableReference> CreateNetCoreReferences()
        {
            foreach (var dllFile in Directory.GetFiles(@"C:\Program Files\dotnet\sdk\NuGetFallbackFolder\microsoft.netcore.app\2.2.0\ref\netcoreapp2.2", "*.dll"))
            {
                yield return MetadataReference.CreateFromFile(dllFile);
            }
        }

        private static IEnumerable<PortableExecutableReference> Test()
        {
            var dir = RuntimeEnvironment.GetRuntimeDirectory();
            foreach (var dllFile in Directory.GetFiles(dir, "*.dll"))
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

        private List<string> FilterInvalidAssembies(List<string> files)
        {
            RemoveStringFromList(files, "System.EnterpriseServices.Wrapper.dll");
            RemoveStringFromList(files, "System.EnterpriseServices.Thunk.dll");
            //RemoveStringFromList(files, "mscorlib.dll");
            return files;
        }

        private void RemoveStringFromList(List<string> items, string contains)
        {
            var item = items.Where(q => q.Contains(contains)).Select(q => q).FirstOrDefault();
            if (item != null)
            {
                items.Remove(item);
            }
        }


        private List<string> GetAssembliesInFolder(string assemblyPath)
        {
            var files = Directory.GetFiles(assemblyPath, "*.dll");

            return files.ToList();
        }

        public Assembly GenerateAssemblyFrom(SyntaxTree syntaxTree, params Assembly[] additionalReferences)
        {
            string assemblyFolder = string.Empty;
            string coreAssemblyFileName = string.Empty;

            assemblyFolder = _FrameworkAssemblyFolder;
            coreAssemblyFileName = "mscorlib.dll";

            PortableExecutableReference objectDef = MetadataReference.CreateFromFile(Path.Combine(assemblyFolder, coreAssemblyFileName));

            var references = new List<PortableExecutableReference>();
            references.Add(objectDef);

            var assemblies = FilterInvalidAssembies(GetAssembliesInFolder(assemblyFolder));
            //if (!string.IsNullOrWhiteSpace(ReferencedAssembliesPath))
            //{
            //    assemblies.AddRange(GetAssembliesInFolder(ReferencedAssembliesPath));
            //}
            foreach (string item in assemblies)
            {
                var reference = MetadataReference.CreateFromFile(item);
                references.Add(reference);
            }
            //var compilation = CSharpCompilation.Create(Path.GetFileNameWithoutExtension(OutputFileNameAndPath), syntaxes, references);
            //compilation = compilation.WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));


            string assemblyName = Path.GetRandomFileName();
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            references.AddRange(additionalReferences.Select(x => MetadataReference.CreateFromFile(x.Location)).ToList());
            references.Add(MetadataReference.CreateFromFile(typeof(IGenerateCodeForChannelClient).Assembly.Location));

            //var references = additionalReferences.Select(x => MetadataReference.CreateFromFile(x.Location)).ToList();
            //references.Add(MetadataReference.CreateFromFile(typeof(IGenerateCodeForChannelClient).Assembly.Location));

            //references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            //references.Add(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));

            //references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")));
            //references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")));
            //references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")));
            //references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));

            //references.Add(MetadataReference.CreateFromFile(typeof(IService).Assembly.Location));
            //references.Add(MetadataReference.CreateFromFile(typeof(Registration).Assembly.Location));
            //references.Add(MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location));
            //references.AddRange(CreateNetCoreReferences());
            //references.AddRange(Test());

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