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
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Get all the entities that inherit from BaseModel
            // and have a state of Added or Modified
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseModel && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

            // For each entity we will set the Audit properties
            foreach (var entityEntry in entries)
            {
                // If the entity state is Added let's set
                // the DateCreated property to UtcNow
                if (entityEntry.State == EntityState.Added)
                {
                    ((BaseModel)entityEntry.Entity).DateCreated = DateTime.UtcNow;
                }
                else
                {
                    // If the state is Modified then we don't want
                    // to modify the DateCreated property
                    // so we set its state as IsModified to false
                    Entry((BaseModel)entityEntry.Entity).Property(p => p.DateCreated).IsModified = false;
                }

                // In any case we always want to set the
                // DateLastUpdated
                ((BaseModel)entityEntry.Entity).DateLastUpdated = DateTime.UtcNow;
            }
            // After we set all the needed properties
            // we call the base implementation of SaveChangesAsync
            // to actually save our entities in the database
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
