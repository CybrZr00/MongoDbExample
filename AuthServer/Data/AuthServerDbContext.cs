using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Shared.Models;

namespace AuthServer.Data
{
    public partial class AuthServerDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public AuthServerDbContext(DbContextOptions<AuthServerDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
