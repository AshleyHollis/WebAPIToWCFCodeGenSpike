using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;

namespace SmarterWCFClient
{
    public static class ServiceChannelClientFactory
    {
        private static IGenerateCodeForChannelClient channelClientCodeGenerator = new ChannelClientCodeGenerator();
        private static IGenerateAssembly assemblyGenerator = new AssemblyGenerator();
       
        public static TChannel Build<TChannel>() where TChannel : class, IDisposable
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(channelClientCodeGenerator.CodeFor<TChannel>());
            var assembly = assemblyGenerator.GenerateAssemblyFrom(syntaxTree, typeof(TChannel).Assembly);
            var clientType = assembly.GetTypes().First(x => typeof(TChannel).IsAssignableFrom(x));
            var ctor = clientType.GetConstructor(new Type[] { });
            return (TChannel)ctor.Invoke(new object[] { });
        }
    }
}