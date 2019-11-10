using SmarterWCFClient;
using SmartHotel.Registration.Wcf.Contracts;
using SmartHotel.Registration.Wcf.Contracts.Data;
using System;

namespace FullDotNetConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Started...");

            var booking = new Booking()
            {
                CustomerName = "CustomerName01",
                Passport = "01",
                CustomerId = string.Format("Cust-{0}", new Random().Next(1, 10000)),
                Address = "Address01",
                Amount = 1,
                From = DateTime.Now,
                To = DateTime.Now.AddDays(1),
                Total = new Random().Next(10, 40) * 100
            };

            using (var client = new ServiceChannelClientFactory().Build<IService>())
            {
                client.PostRegister(booking);
            }

            Console.WriteLine("Completed.");
            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }
    }
}
