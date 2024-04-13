using Microsoft.EntityFrameworkCore;

using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;

using Mongotest.Models.V1;

using System.Numerics;
using System.Text.Json;

namespace Mongotest.Data
{
    public class ApplicationEFContext : DbContext
    {
        public DbSet<PersonModel> People { get; set; }
        public DbSet<HistoryModelEF> Histories { get; set; }
        public DbSet<HistoryItem> HistoryItems { get; set; }
        public static ApplicationEFContext Create(IMongoDatabase database) =>
            new(new DbContextOptionsBuilder<ApplicationEFContext>()
        .UseMongoDB(database.Client, database.DatabaseNamespace.DatabaseName)
        .Options);
        public ApplicationEFContext(DbContextOptions<ApplicationEFContext> options) : base(options)
        {
            //Database.EnsureCreatedAsync();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<PersonModel>().ToCollection(nameof(PersonModel));
            modelBuilder.Entity<HistoryModel<BaseModel>>().ToCollection("History");
            modelBuilder.Entity<HistoryItem>().ToCollection(nameof(HistoryItem));
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
                //if (entityEntry.GetType() != typeof(BaseModel)) return 0;
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
        //private void AddHistory(Guid id, BaseModel model)
        //{

        //    // First check if there is a history for the model
        //    var historyModel = Histories.Include(x => x.HistoryEntries).SingleOrDefault(x => x.ModelId == model.Id);
        //    if (historyModel is not null)
        //    {
        //        historyModel.DateLastUpdated = DateTime.UtcNow;
        //        if (historyModel.HistoryEntries is null)
        //        {
        //            historyModel.HistoryEntries = new();
        //        }
        //        var json = JsonSerializer.Serialize(model);
        //        var list = historyModel.HistoryEntries.ToList();
        //        var item = new HistoryItem
        //        {
        //            Id = Guid.NewGuid(),
        //            ModelType = model.GetType().Name,
        //            ModelJson = json
        //        };
        //        historyModel.HistoryEntries.Add(item);
        //        Histories.Update(historyModel);
        //    }
        //    else
        //    {
        //        var history = new HistoryModelEF
        //        {
        //            Id = Guid.NewGuid(),
        //            ModelId = id,
        //            Notes = "",
        //            DateLastUpdated = DateTime.UtcNow,
        //            DateCreated = DateTime.UtcNow
        //        };
        //        if (history.HistoryEntries is null)
        //        {
        //            history.HistoryEntries = new();
        //        }
        //        var json = JsonSerializer.Serialize(model);
        //        var item = new HistoryItem
        //        {
        //            Id = Guid.NewGuid(),
        //            ModelType = model.GetType().Name,
        //            ModelJson = json
        //        };
        //        history.HistoryEntries.Add(item);   
        //        Histories.Add(history);
        //    }
        //}
    }
}
