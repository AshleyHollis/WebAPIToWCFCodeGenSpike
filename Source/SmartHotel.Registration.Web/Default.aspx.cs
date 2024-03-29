﻿using Newtonsoft.Json;
using SmarterWCFClient;
using SmartHotel.Registration.Models;
using SmartHotel.Registration.Services;
using SmartHotel.Registration.Wcf.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace SmartHotel.Registration
{
    public partial class _Default : Page
    {
        protected async void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;

            var isStoreKPIEnabled = Environment.GetEnvironmentVariable("UseStoreKPIsStatefulService");
            if (isStoreKPIEnabled == bool.FalseString)
            {
                ShowKPIsButton.Visible = false;
            }

            using (var client = ServiceChannelClientFactory.Build<IService>())
            {
                var registrations = client.GetTodayRegistrations();

                RegistrationGrid.DataSource = registrations;
                RegistrationGrid.DataBind();
            }
        }

        protected void RegistrationGrid_SelectedIndexChanged(Object sender, EventArgs e)
        {
            GridViewRow row = RegistrationGrid.SelectedRow;

            var registrationId = RegistrationGrid.DataKeys[RegistrationGrid.SelectedIndex]["Id"];
            var registrationType = RegistrationGrid.DataKeys[RegistrationGrid.SelectedIndex]["Type"].ToString();

            if (registrationType == "CheckIn")
            {
                Response.Redirect($"Checkin.aspx?registration={registrationId}");
            }

            if (registrationType == "CheckOut")
            {
                Response.Redirect($"Checkout.aspx?registration={registrationId}");
            }
        }

        protected void RegistrationGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;
            e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(RegistrationGrid, "Select$" + e.Row.RowIndex);
        }

        protected void AddRegisterBtn_Click(Object sender, EventArgs e)
        {
            Response.Redirect($"Register.aspx");
        }

        protected void ShowKPIsBtn_Click(Object sender, EventArgs e)
        {
            Response.Redirect($"RegistrationKPIs.aspx");
        }
    }
}