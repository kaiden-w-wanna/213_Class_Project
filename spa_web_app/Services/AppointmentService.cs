using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using spa_web_app.Data;
using spa_web_app.Models;

namespace spa_web_app.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly spa_web_appContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;

        public AppointmentService(spa_web_appContext dbContext, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<Appointment> CreateAppointmentAsync(
            string customerId,
            string serviceName,
            DateTime startTime,
            DateTime? endTime,
            decimal? price,
            string? therapistId)
        {
            // (Optional) Validate therapist is actually in the Therapist role
            if (!string.IsNullOrWhiteSpace(therapistId))
            {
                var therapist = await _userManager.FindByIdAsync(therapistId);
                if (therapist == null || !await _userManager.IsInRoleAsync(therapist, "Therapist"))
                {
                    throw new InvalidOperationException("Selected user is not a valid therapist.");
                }
            }

            var appointment = new Appointment
            {
                CustomerId = customerId,
                TherapistId = therapistId,
                ServiceName = serviceName,
                StartTime = startTime,
                EndTime = endTime,
                Price = price,
                Status = "Booked",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Appointments.Add(appointment);
            await _dbContext.SaveChangesAsync();

            return appointment;
        }

        public async Task<IReadOnlyList<Appointment>> GetAppointmentsForUserAsync(ClaimsPrincipal user)
        {
            var identityUser = await _userManager.GetUserAsync(user);
            if (identityUser == null)
                return Array.Empty<Appointment>();

            var roles = await _userManager.GetRolesAsync(identityUser);
            bool isStaff = roles.Any(r =>
                r == "Therapist" ||
                r == "Receptionist" ||
                r == "Manager" ||
                r == "Admin");

            IQueryable<Appointment> query = _dbContext.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Therapist)
                .OrderBy(a => a.StartTime);

            if (!isStaff)
            {
                // Customer: only see their own
                query = query.Where(a => a.CustomerId == identityUser.Id);
            }
            else if (roles.Contains("Therapist"))
            {
                // Therapist: only see their own appointments
                query = query.Where(a => a.TherapistId == identityUser.Id);
            }

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<Appointment>> GetUpcomingAppointmentsForUserAsync(
            ClaimsPrincipal user,
            DateTime? fromUtc = null)
        {
            var identityUser = await _userManager.GetUserAsync(user);
            if (identityUser == null)
                return Array.Empty<Appointment>();

            var roles = await _userManager.GetRolesAsync(identityUser);
            bool isStaff = roles.Any(r =>
                r == "Therapist" ||
                r == "Receptionist" ||
                r == "Manager" ||
                r == "Admin");

            var start = fromUtc ?? DateTime.UtcNow;

            IQueryable<Appointment> query = _dbContext.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Therapist)
                .Where(a => a.StartTime >= start)
                .OrderBy(a => a.StartTime);

            if (!isStaff)
            {
                query = query.Where(a => a.CustomerId == identityUser.Id);
            }
            else if (roles.Contains("Therapist"))
            {
                query = query.Where(a => a.TherapistId == identityUser.Id);
            }

            return await query.ToListAsync();
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId, ClaimsPrincipal user)
        {
            var identityUser = await _userManager.GetUserAsync(user);
            if (identityUser == null)
                return false;

            var roles = await _userManager.GetRolesAsync(identityUser);
            bool isStaff = roles.Any(r =>
                r == "Therapist" ||
                r == "Receptionist" ||
                r == "Manager" ||
                r == "Admin");

            var appointment = await _dbContext.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                return false;

            if (!isStaff && appointment.CustomerId != identityUser.Id)
            {
                // Customer trying to cancel someone else's
                return false;
            }

            if (appointment.Status == "Cancelled")
                return true;

            appointment.Status = "Cancelled";
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IReadOnlyList<IdentityUser>> GetAvailableTherapistsAsync(
            DateTime startTime,
            DateTime endTime)
        {
            var busyTherapistIds = await _dbContext.Appointments
                .Where(a =>
                    a.TherapistId != null &&
                    a.StartTime < endTime &&
                    (a.EndTime ?? a.StartTime.AddHours(1)) > startTime &&
                    a.Status != "Cancelled")
                .Select(a => a.TherapistId!)
                .ToListAsync();

            var therapists = await _userManager.GetUsersInRoleAsync("Therapist");

            return therapists
                .Where(t => !busyTherapistIds.Contains(t.Id))
                .ToList();
        }
    }
}
