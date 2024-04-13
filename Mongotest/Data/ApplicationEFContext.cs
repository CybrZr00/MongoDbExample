using Microsoft.EntityFrameworkCore;

using MongoDB.Driver;

using Mongotest.Models.V1;

namespace Mongotest.Data
{
    public class ApplicationEFContext : DbContext
    {
        public DbSet<PersonModel> People { get; set; }
        public DbSet<HistoryModel<BaseModel>> Histories { get; set; }
        public ApplicationEFContext(DbContextOptions<ApplicationEFContext> options) : base(options)
        {
            Database.EnsureCreatedAsync();
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
                if (entityEntry.GetType() != typeof(BaseModel)) return 0;
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
                AddHistory(((BaseModel)entityEntry.Entity).Id, (BaseModel)entityEntry.Entity);
            }
            
            // After we set all the needed properties
            // we call the base implementation of SaveChangesAsync
            // to actually save our entities in the database
            return await base.SaveChangesAsync(cancellationToken);
        }
        private void AddHistory<T>(string id, T model) where T : BaseModel
        {

            // First check if there is a history for the model
            var historyModel = this.Histories.Include(x => x.Models).SingleOrDefault(x => x.Id == id);
            if (historyModel is not null)
            {
                historyModel.DateLastUpdated = DateTime.UtcNow;
                if (historyModel.Models is null)
                {
                    historyModel.Models = new();
                }
                historyModel.Models.Add(model);
                Histories.Update(historyModel);
            }
            else
            {
                var history = new HistoryModel<T>
                {
                    Id = id,
                    ModelId = id,
                    Notes = "",
                    DateLastUpdated = DateTime.UtcNow,
                    DateCreated = DateTime.UtcNow
                };
                if (history.Models is null)
                {
                    history.Models = new List<T>();
                }
                history.Models.Add(model);                
            }
        }
    }
}
