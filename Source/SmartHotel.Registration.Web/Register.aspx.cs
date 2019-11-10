using SmartHotel.Registration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SmarterWCFClient;
using SmartHotel.Registration.Wcf.Contracts;
using SmartHotel.Registration.Wcf.Contracts.Data;

namespace SmartHotel.Registration
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void AddRegisterBtn_Click(Object sender, EventArgs e)
        {
            var booking = new Booking()
            {
                CustomerName = CustomerName.Value,
                Passport = Passport.Value,
                CustomerId = string.Format("Cust-{0}", new Random().Next(1, 10000)),
                Address = Address.Value,
                Amount = int.Parse(Amount.Value),
                From = Calendar1.SelectedDate,
                To = Calendar2.SelectedDate,
                Total = new Random().Next(10, 40) * 100
            };

            using (var client = new ServiceChannelClientFactory().Build<IService>())
            {
                client.PostRegister(booking);
            }

            Response.Redirect($"Default.aspx");
        }
        protected void CancelBtn_Click(Object sender, EventArgs e)
        {
            Response.Redirect($"Default.aspx");
        }
    }
}