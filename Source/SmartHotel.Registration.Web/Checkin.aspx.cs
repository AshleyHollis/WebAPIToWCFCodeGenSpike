﻿using SmarterWCFClient;
using SmartHotel.Registration.Wcf.Contracts;
using SmartHotel.Registration.Wcf.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SmartHotel.Registration
{
    public partial class Checkin : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;

            var registrationProvided = 
                int.TryParse(Request.QueryString["registration"], out int registrationId);

            if (!registrationProvided)
                Response.Redirect("Default.aspx");

            using (var client = ServiceChannelClientFactory.Build<IService>())
            {
                var checkin = client.GetCheckin(registrationId);

                CustomerName.Value = checkin.CustomerName;
                Passport.Value = checkin.Passport;
                CustomerId.Value = checkin.CustomerId;
                Address.Value = checkin.Address;
                Amount.Value = checkin.Amount.ToString();
            }
        }

        protected void BackBtn_Click(Object sender, EventArgs e)
        {
            Response.Redirect($"Default.aspx");
        }
    }
}