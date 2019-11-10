using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SmarterWCFClient
{
    public interface IGenerateCodeForChannelClient
    {
        string CodeFor<TChannel>();
    }

    public class ChannelClientCodeGenerator : IGenerateCodeForChannelClient
    {
        private static void AppendIDefaultIDisposableImplementation(StringBuilder sb)
        {
            sb.Append(@"
                        #region IDisposable Support
                        private bool disposedValue = false; // To detect redundant calls

                        protected virtual void Dispose(bool disposing)
                        {
                            if (!disposedValue)
                            {
                                if (disposing)
                                {
                                    // TODO: dispose managed state (managed objects).
                                }

                                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                                // TODO: set large fields to null.

                                disposedValue = true;
                            }
                        }

                        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
                        // ~Service()
                        // {
                        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                        //   Dispose(false);
                        // }

                        // This code added to correctly implement the disposable pattern.
                        public void Dispose()
                        {
                            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                            Dispose(true);
                            // TODO: uncomment the following line if the finalizer is overridden above.
                            // GC.SuppressFinalize(this);
                        }
                        #endregion
                ");
        }

        // loop over the interface methods and generate the code for each
        private static void AppendMethods(Type channelType, StringBuilder sb)
        {
            foreach (var methodInfo in channelType.GetMethods())
            {
                sb.Append(GenerateMethodProxy(methodInfo));
            }
        }

        private static string GenerateMethodProxy(MethodInfo methodInfo)
        {
            var methodReturnTypeIsVoid = methodInfo.ReturnType == typeof(void);
            var methodReturnTypeName = Helpers.CSharpName(methodInfo.ReturnType);

            return $@"
            public {methodReturnTypeName} {methodInfo.Name}({GenerateMethodParameters(methodInfo.GetParameters())}) 
            {{
                { (!methodReturnTypeIsVoid ? "return " : "") } InvokeMethod(x => x.{methodInfo.Name}({string.Join(", ", methodInfo.GetParameters().Select(x => x.Name))}));
            }}
            ";
        }

        private static string Normalize(string channelName)
        {
            if (channelName.Substring(0, 1) == "I")
            {
                return channelName.Substring(1, channelName.Length - 1);
            }

            return channelName;
        }

        private static string GenerateMethodParameters(ParameterInfo[] parameters)
        {
            return string.Join(", ", parameters.Select(x => $"{x.ParameterType.Name} {x.Name}"));
        }

        public string CodeFor<TChannel>()
        {
            var channelType = typeof(TChannel);
            var sb = new StringBuilder();

            // the usings
            sb.Append($@"
using System;
using SmarterWCFClient;
using System.Collections.Generic;
using SmartHotel.Registration.Wcf.Contracts.Data;
using SmartHotel.Registration.Wcf.Contracts.Models;
using {channelType.Namespace};
");

            sb.Append("namespace SmarterWCFClient {");
            sb.Append($@"public class {Normalize(channelType.Name)}ServiceClient : ServiceChannelClient<{channelType.Name}>, {channelType.Name} {{");
            AppendMethods(channelType, sb);
            AppendIDefaultIDisposableImplementation(sb);
            sb.Append("}}");

            return sb.ToString();
        }
    }
}