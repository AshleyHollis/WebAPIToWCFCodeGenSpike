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

    public static class Helpers
    {
        public static string GetOriginalName(this Type type)
        {
            string TypeName = type.FullName.Replace(type.Namespace + ".", "");//Removing the namespace

            var provider = System.CodeDom.Compiler.CodeDomProvider.CreateProvider("CSharp"); //You can also use "VisualBasic"
            var reference = new System.CodeDom.CodeTypeReference(TypeName);

            return provider.GetTypeOutput(reference);
        }

        /// <summary>
        /// Get full type name with full namespace names
        /// </summary>
        /// <param name="type">
        /// The type to get the C# name for (may be a generic type or a nullable type).
        /// </param>
        /// <returns>
        /// Full type name, fully qualified namespaces
        /// </returns>
        public static string CSharpName(this Type type)
        {
            Type nullableType = Nullable.GetUnderlyingType(type);
            string nullableText;
            if (nullableType != null)
            {
                type = nullableType;
                nullableText = "?";
            }
            else
            {
                nullableText = string.Empty;
            }

            if (type.IsGenericType)
            {
                return string.Format(
                    "{0}<{1}>{2}",
                    type.Name.Substring(0, type.Name.IndexOf('`')),
                    string.Join(", ", type.GetGenericArguments().Select(ga => ga.CSharpName())),
                    nullableText);
            }

            switch (type.Name)
            {
                case "String":
                    return "string";
                case "Int32":
                    return "int" + nullableText;
                case "Decimal":
                    return "decimal" + nullableText;
                case "Object":
                    return "object" + nullableText;
                case "Void":
                    return "void" + nullableText;
                default:
                    return (string.IsNullOrWhiteSpace(type.FullName) ? type.Name : type.FullName) + nullableText;
            }
        }
    }

    public class ChannelClientCodeGenerator : IGenerateCodeForChannelClient
    {
        

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
            sb.Append($@"public class {normalize(channelType.Name)}ServiceClient : ServiceChannelClient<{channelType.Name}>, {channelType.Name} {{");

            // loop over the interface methods and generate the code for each
            foreach (var methodInfo in channelType.GetMethods())
            {
                sb.Append(generateMethodProxy(methodInfo));
            }

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

            sb.Append("}}");

            return sb.ToString();
        }

        private string generateMethodProxy(MethodInfo methodInfo)
        {
            var methodReturnTypeIsVoid = methodInfo.ReturnType == typeof(void);
            var methodReturnTypeName = Helpers.CSharpName(methodInfo.ReturnType);

            //            return $@"
            //public {methodReturnTypeName} {methodInfo.Name}({generateMethodParameters(methodInfo.GetParameters())}) {{
            //return InvokeMethod(x => x.{methodInfo.Name}({string.Join(", ", methodInfo.GetParameters().Select(x => x.Name))}));
            //}}
            //";

            return $@"
            public {methodReturnTypeName} {methodInfo.Name}({generateMethodParameters(methodInfo.GetParameters())}) 
            {{
                { (!methodReturnTypeIsVoid ? "return " : "") } InvokeMethod(x => x.{methodInfo.Name}({string.Join(", ", methodInfo.GetParameters().Select(x => x.Name))}));
            }}
            ";
        }

        private string normalize(string channelName)
        {
            if (channelName.Substring(0, 1) == "I")
            {
                return channelName.Substring(1, channelName.Length - 1);
            }

            return channelName;
        }

        private string generateMethodParameters(ParameterInfo[] parameters)
        {
            return string.Join(", ", parameters.Select(x => $"{x.ParameterType.Name} {x.Name}"));
        }
    }
}