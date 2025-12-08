using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace spa_web_app.Data
{
    public class spa_web_appContext(DbContextOptions<spa_web_appContext> options) : IdentityDbContext<IdentityUser>(options)
    {
    }
}
