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
            decimal? price)
        {
            var appointment = new Appointment
            {
                CustomerId = customerId,
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
            {
                return Array.Empty<Appointment>();
            }

            var roles = await _userManager.GetRolesAsync(identityUser);
            bool isEmployee = roles.Contains("Employee");

            IQueryable<Appointment> query = _dbContext.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Employee)
                .OrderBy(a => a.StartTime);

            if (!isEmployee)
            {
                // Customer: only their own appointments
                query = query.Where(a => a.CustomerId == identityUser.Id);
            }

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<Appointment>> GetUpcomingAppointmentsForUserAsync(
            ClaimsPrincipal user,
            DateTime? fromUtc = null)
        {
            var identityUser = await _userManager.GetUserAsync(user);
            if (identityUser == null)
            {
                return Array.Empty<Appointment>();
            }

            var roles = await _userManager.GetRolesAsync(identityUser);
            bool isEmployee = roles.Contains("Employee");
            var start = fromUtc ?? DateTime.UtcNow;

            IQueryable<Appointment> query = _dbContext.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Employee)
                .Where(a => a.StartTime >= start)
                .OrderBy(a => a.StartTime);

            if (!isEmployee)
            {
                query = query.Where(a => a.CustomerId == identityUser.Id);
            }

            return await query.ToListAsync();
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId, ClaimsPrincipal user)
        {
            var identityUser = await _userManager.GetUserAsync(user);
            if (identityUser == null)
            {
                return false;
            }

            var roles = await _userManager.GetRolesAsync(identityUser);
            bool isEmployee = roles.Contains("Employee");

            var appointment = await _dbContext.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
            if (appointment == null)
            {
                return false;
            }

            if (!isEmployee && appointment.CustomerId != identityUser.Id)
            {
                // Customer trying to cancel someone else’s appointment
                return false;
            }

            if (appointment.Status == "Cancelled")
            {
                return true; // already cancelled, treat as success
            }

            appointment.Status = "Cancelled";
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}

