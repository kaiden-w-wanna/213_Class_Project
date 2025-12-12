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

        // --------------------------
        // CREATE
        // --------------------------
        public async Task<Appointment> CreateAppointmentAsync(
            string customerId,
            string serviceName,
            DateTime startTime,
            DateTime? endTime,
            decimal? price,
            string? therapistId)
        {
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

        // --------------------------
        // MY APPOINTMENTS
        // --------------------------
        public async Task<IReadOnlyList<Appointment>> GetAppointmentsForUserAsync(ClaimsPrincipal user)
        {
            var identityUser = await _userManager.GetUserAsync(user);
            if (identityUser == null)
                return Array.Empty<Appointment>();

            var roles = await _userManager.GetRolesAsync(identityUser);
            bool isTherapist = roles.Contains("Therapist");

            IQueryable<Appointment> query = _dbContext.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Therapist)
                .OrderBy(a => a.StartTime);

            // Therapists see appointments assigned TO THEM
            if (isTherapist)
            {
                query = query.Where(a => a.TherapistId == identityUser.Id);
            }
            else
            {
                // Everyone else (customer, receptionist, manager, admin)
                query = query.Where(a => a.CustomerId == identityUser.Id);
            }

            return await query.ToListAsync();
        }

        // --------------------------
        // MY UPCOMING APPOINTMENTS
        // --------------------------
        public async Task<IReadOnlyList<Appointment>> GetUpcomingAppointmentsForUserAsync(ClaimsPrincipal user, DateTime? fromUtc = null)
        {
            var identityUser = await _userManager.GetUserAsync(user);
            if (identityUser == null)
                return Array.Empty<Appointment>();

            var roles = await _userManager.GetRolesAsync(identityUser);
            bool isTherapist = roles.Contains("Therapist");

            var start = fromUtc ?? DateTime.UtcNow;

            IQueryable<Appointment> query = _dbContext.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Therapist)
                .Where(a => a.StartTime >= start)
                .OrderBy(a => a.StartTime);

            if (isTherapist)
            {
                query = query.Where(a => a.TherapistId == identityUser.Id);
            }
            else
            {
                query = query.Where(a => a.CustomerId == identityUser.Id);
            }

            return await query.ToListAsync();
        }

        // --------------------------
        // ALL APPOINTMENTS (STAFF ONLY)
        // --------------------------
        public async Task<IReadOnlyList<Appointment>> GetAllAppointmentsForDateAsync(DateTime date, ClaimsPrincipal user)
        {
            var identityUser = await _userManager.GetUserAsync(user);
            if (identityUser == null)
                return Array.Empty<Appointment>();

            var roles = await _userManager.GetRolesAsync(identityUser);
            bool canViewAll = roles.Any(r => r == "Receptionist" || r == "Manager" || r == "Admin");

            if (!canViewAll)
                return Array.Empty<Appointment>();

            var startOfDay = date.Date;
            var endOfDay = date.Date.AddDays(1);

            return await _dbContext.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Therapist)
                .Where(a => a.StartTime >= startOfDay && a.StartTime < endOfDay)
                .OrderBy(a => a.StartTime)
                .ToListAsync();
        }

        // --------------------------
        // CANCEL APPOINTMENT
        // --------------------------
        public async Task<bool> CancelAppointmentAsync(int appointmentId, ClaimsPrincipal user)
        {
            var identityUser = await _userManager.GetUserAsync(user);
            if (identityUser == null)
                return false;

            var roles = await _userManager.GetRolesAsync(identityUser);
            bool isStaff = roles.Any(r =>
                r == "Therapist" || r == "Receptionist" || r == "Manager" || r == "Admin");

            var appointment = await _dbContext.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
            if (appointment == null)
                return false;

            if (!isStaff && appointment.CustomerId != identityUser.Id)
                return false;

            if (appointment.Status == "Cancelled")
                return true;

            appointment.Status = "Cancelled";
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // --------------------------
        // AVAILABLE THERAPISTS
        // --------------------------
        public async Task<IReadOnlyList<IdentityUser>> GetAvailableTherapistsAsync(DateTime startTime, DateTime endTime)
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
