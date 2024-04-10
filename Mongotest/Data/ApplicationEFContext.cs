using Microsoft.EntityFrameworkCore;

using Mongotest.Models.V1;

namespace Mongotest.Data
{
    public class ApplicationEFContext : DbContext
    {
        public DbSet<PersonModel> People { get; set; }
        public ApplicationEFContext(DbContextOptions<ApplicationEFContext> options) : base(options)
        {
               
        }
    }
}
