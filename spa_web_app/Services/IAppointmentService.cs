using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using spa_web_app.Models;

namespace spa_web_app.Services
{
    public interface IAppointmentService
    {
        Task<Appointment> CreateAppointmentAsync(
            string customerId,
            string serviceName,
            DateTime startTime,
            DateTime? endTime,
            decimal? price);

        /// <summary>
        /// Returns all appointments the given user is allowed to see.
        /// - Customer: only their own appointments.
        /// - Employee: all appointments.
        /// </summary>
        Task<IReadOnlyList<Appointment>> GetAppointmentsForUserAsync(ClaimsPrincipal user);

        /// <summary>
        /// Same as GetAppointmentsForUserAsync, but only those on/after fromUtc (default = now).
        /// </summary>
        Task<IReadOnlyList<Appointment>> GetUpcomingAppointmentsForUserAsync(
            ClaimsPrincipal user,
            DateTime? fromUtc = null);

        /// <summary>
        /// Cancels an appointment if the user is allowed.
        /// - Customer: may only cancel their own.
        /// - Employee: may cancel any.
        /// Returns true if something was changed.
        /// </summary>
        Task<bool> CancelAppointmentAsync(int appointmentId, ClaimsPrincipal user);
    }
}

