//using SmartHotel.Registration.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartHotel.Registration
{
    public static class ServiceClientFactory
    {
        // TODO: Need to replace with proper WCF client.
        public static object NewServiceClient()
        {
            //var client = new ServiceClient();
            //var uri = Environment.GetEnvironmentVariable("WcfServiceUri");

            //if (!string.IsNullOrEmpty(uri))
            //{
            //    client.Endpoint.Address = new System.ServiceModel.EndpointAddress(uri);
            //}

            //return client;
            return new object();
        }
    }
}