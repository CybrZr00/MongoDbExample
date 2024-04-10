using Microsoft.EntityFrameworkCore;

namespace Mongotest.Data
{
    public class ApplicationEFContext : DbContext
    {
        public ApplicationEFContext(DbContextOptions<ApplicationEFContext> options) : base(options)
        {
               
        }
    }
}
