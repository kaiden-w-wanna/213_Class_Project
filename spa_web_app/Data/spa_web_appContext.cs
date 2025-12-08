using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using spa_web_app.Models;

namespace spa_web_app.Data
{
    public class spa_web_appContext(DbContextOptions<spa_web_appContext> options) : IdentityDbContext<IdentityUser>(options)
    {
        public DbSet<Appointment> Appointments { get; set; } = default!;
    }
}
