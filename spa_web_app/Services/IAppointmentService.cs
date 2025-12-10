using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
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
            decimal? price,
            string? therapistId);

        /// <summary>
        /// Returns all appointments the given user is allowed to see.
        /// - Customer: only their own appointments.
        /// - Therapist/Receptionist/Manager/Admin: all appointments.
        /// </summary>
        Task<IReadOnlyList<Appointment>> GetAppointmentsForUserAsync(ClaimsPrincipal user);

        Task<IReadOnlyList<Appointment>> GetUpcomingAppointmentsForUserAsync(
            ClaimsPrincipal user,
            DateTime? fromUtc = null);

        /// <summary>
        /// Cancels an appointment if the user is allowed.
        /// - Customer: may only cancel their own.
        /// - Therapist/Receptionist/Manager/Admin: may cancel any.
        /// </summary>
        Task<bool> CancelAppointmentAsync(int appointmentId, ClaimsPrincipal user);

        /// <summary>
        /// Returns therapists available in the given time window.
        /// </summary>
        Task<IReadOnlyList<IdentityUser>> GetAvailableTherapistsAsync(
            DateTime startTime,
            DateTime endTime);
    }
}
