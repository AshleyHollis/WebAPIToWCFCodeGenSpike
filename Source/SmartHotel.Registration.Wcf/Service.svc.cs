using SmartHotel.Registration.Wcf.Contracts;
using SmartHotel.Registration.Wcf.Contracts.Data;
using SmartHotel.Registration.Wcf.Contracts.Models;
using SmartHotel.Registration.Wcf.Data;
using SmartHotel.Registration.Wcf.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace SmartHotel.Registration.Wcf
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class Service : IService
    {
        public IEnumerable<Contracts.Models.Registration> GetRegistrations()
        {
            using (var db = new BookingsDbContext())
            {
                var registrations = db.Bookings
                    .Select(BookingToCheckin);

                return registrations.ToList();
            }
        }

        public IEnumerable<Contracts.Models.Registration> GetTodayRegistrations()
        {
            using (var db = new BookingsDbContext())
            {
                var checkins = db.Bookings
                .Where(b => b.From == DateTime.Today)
                .Select(BookingToCheckin);

                var checkouts = db.Bookings
                    .Where(b => b.To == DateTime.Today)
                    .Select(BookingToCheckout);

                var registrations = checkins.Concat(checkouts).OrderBy(r => r.Date);
                return registrations.ToList();
            }
        }

        public RegistrationDaySummary GetTodayRegistrationSummary()
        {
            using (var db = new BookingsDbContext())
            {
                var totalCheckins = db.Bookings
                .Count(b => b.From == DateTime.Today);

                var totalCheckouts = db.Bookings
                    .Count(b => b.To == DateTime.Today);

                var summary = new RegistrationDaySummary
                {
                    Date = DateTime.Today,
                    CheckIns = totalCheckins,
                    CheckOuts = totalCheckouts
                };

                return summary;
            }
        }

        public Contracts.Models.Registration GetCheckin(int registrationId)
        {
            using (var db = new BookingsDbContext())
            {
                var checkin = db.Bookings
                .Where(b => b.Id == registrationId)
                .Select(BookingToCheckin)
                .First();

                return checkin;
            }
        }

        public Contracts.Models.Registration GetCheckout(int registrationId)
        {
            using (var db = new BookingsDbContext())
            {
                var checkout = db.Bookings
                .Where(b => b.Id == registrationId)
                .Select(BookingToCheckin)
                .First();

                return checkout;
            }
        }

        public void PostRegister(Booking booking)
        {
            using (var db = new BookingsDbContext())
            {
                var checkin = db.Bookings.Add(booking);
                db.SaveChanges();
            }

            var isStoreKPIEnabled = Environment.GetEnvironmentVariable("UseStoreKPIsStatefulService");
            if (isStoreKPIEnabled == bool.TrueString)
            {
                UpdateRegistrationKPIStatefulService(booking);
            }
        }

        private Contracts.Models.Registration BookingToCheckin(Booking booking)
        {
            return new Contracts.Models.Registration
            {
                Id = booking.Id,
                Type = "CheckIn",
                Date = booking.From,
                CustomerId = booking.CustomerId,
                CustomerName = booking.CustomerName,
                Passport = booking.Passport,
                Address = booking.Address,
                Amount = booking.Amount,
                From = booking.From,
                To = booking.To,
                Total = booking.Total
            };
        }

        private Contracts.Models.Registration BookingToCheckout(Booking booking)
        {
            return new Contracts.Models.Registration
            {
                Id = booking.Id,
                Type = "CheckOut",
                Date = booking.To,
                CustomerId = booking.CustomerId,
                CustomerName = booking.CustomerName,
                Passport = booking.Passport,
                From = booking.From,
                To = booking.To,
                Address = booking.Address,
                Amount = booking.Amount,
                Total = booking.Total
            };
        }

        private void UpdateRegistrationKPIStatefulService(Booking booking)
        {
            var registrationKPIService = new RegistrationKPIService();
            registrationKPIService.SendBookingInfo(booking).Wait();
        }

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
    }
}
