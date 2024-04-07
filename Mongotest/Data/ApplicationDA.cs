using MongoDB.Bson;
using MongoDB.Driver;
using Mongotest.Models.V1;

namespace Mongotest.Data
{
    public class ApplicationDA : IApplicationDA
    {

        public ApplicationDA()
        {

        }
        private IMongoCollection<T> ConnectDb<T>(in string collectionName)
        {
            var client = new MongoClient(Utilities.Database.ConnectionString);
            var database = client.GetDatabase(Utilities.Database.DatabaseName);
            return database.GetCollection<T>(collectionName);
        }
        public async Task<List<T>> GetAllAsync<T>(string collectionName)
        {
            var collection = ConnectDb<T>(collectionName);
            var ls = await collection.FindAsync(_ => true);
            return await ls.ToListAsync();
        }
        public async Task<T> GetOneAsync<T>(string id, string collectionName) where T : BaseModel
        {
            var collection = ConnectDb<T>(collectionName);
            var ls = await collection.FindAsync(x => x.Id == id);
            return await ls.FirstOrDefaultAsync();
        }
        // add the remaining crud operations
        public async Task<T> CreateAsync<T>(T model, string collectionName) where T : BaseModel
        {
            model.DateCreated = DateTime.UtcNow;
            model.DateLastUpdated = DateTime.UtcNow;
            var collection = ConnectDb<T>(collectionName);
            await collection.InsertOneAsync(model);
            try
            {
                await AddHistory(model.Id, model, "Created", collectionName);
            }
            catch (Exception ex)
            {
                throw;
            }
            

            return model;
        }
        public async Task<T> UpsertAsync<T>(string id, T model, string collectionName) where T : BaseModel
        {
            var collection = ConnectDb<T>(collectionName);
            model.DateLastUpdated = DateTime.UtcNow;
            await collection.ReplaceOneAsync(x => x.Id == id, model, new ReplaceOptions { IsUpsert = true });

            await AddHistory(id, model, "Upserted", collectionName);

            return model;
        }
        public async Task DeleteAsync<T>(string id, string collectionName) where T : BaseModel
        {
            var collection = ConnectDb<T>(collectionName);
            await collection.DeleteOneAsync(x => x.Id == id);
        }
        public async Task<List<T>> FilterEquals<T>(string field, string value, string collectionName) where T : BaseModel
        {
            var collection = ConnectDb<T>(collectionName);
            var filter = Builders<T>.Filter.Eq(field, value);
            var ls = await collection.FindAsync(filter);
            return await ls.ToListAsync();
        }
        public async Task<List<T>> FilterContains<T>(string field, string value, string collectionName) where T : BaseModel
        {
            var collection = ConnectDb<T>(collectionName);
            var filter = Builders<T>.Filter.Regex(field, new BsonRegularExpression(value, "i"));
            var ls = await collection.FindAsync(filter);
            return await ls.ToListAsync();
        }
        public async Task<List<T>> FilterGreaterThan<T>(string field, string value, string collectionName) where T : BaseModel
        {

            var collection = ConnectDb<T>(collectionName);
            var filter = Builders<T>.Filter.Gt(field, value);
            var ls = await collection.FindAsync(filter);
            return await ls.ToListAsync();
        }
        public async Task<List<T>> FilterLessThan<T>(string field, string value, string collectionName) where T : BaseModel
        {
            var collection = ConnectDb<T>(collectionName);
            var filter = Builders<T>.Filter.Lt(field, value);
            var ls = await collection.FindAsync(filter);
            return await ls.ToListAsync();
        }
        public async Task<List<T>> FilterByField<T>(string field, bool exists, string collectionName) where T : BaseModel
        {
            var collection = ConnectDb<T>(collectionName);
            var filter = Builders<T>.Filter.Exists(field, exists);
            var ls = await collection.FindAsync(filter);
            return await ls.ToListAsync();
        }
        public async Task DeleteAll<T>(string collectionName) where T : BaseModel
        {
            var collection = ConnectDb<T>(collectionName);
            await collection.DeleteManyAsync(_ => true);
        }
        public async Task<bool> Any<T>(string collectionName) where T : BaseModel
        {
            var collection = ConnectDb<T>(collectionName);
            var ls = await collection.FindAsync(_ => true);
            return await ls.AnyAsync();
        }
        private async Task AddHistory<T>(string id, T model, string notes, string collectionName) where T : BaseModel
        {

            var collectionString = $"HistoryModel<{collectionName}>";
            var collection = ConnectDb<HistoryModel<T>>(collectionString);

            // First check if there is a history for the model
            var ls = await collection.FindAsync(f => f.ModelId != null && f.ModelId == id);
            if (ls.Any())
            {
                var historyModel = await ls.FirstOrDefaultAsync();
                historyModel.DateLastUpdated = DateTime.UtcNow;
                if (historyModel.Models is null)
                {
                    historyModel.Models = new List<T>();
                }
                historyModel.Models.Add(model);
                await collection.ReplaceOneAsync(f => f.ModelId != null && f.ModelId == id, historyModel, new ReplaceOptions { IsUpsert = true });
            }
            else
            {
                var history = new HistoryModel<T>
                {
                    Id = id,
                    ModelId = id,
                    Notes = notes,
                    DateLastUpdated = DateTime.UtcNow,
                    DateCreated = DateTime.UtcNow
                };
                if (history.Models is null)
                {
                    history.Models = new List<T>();
                }
                history.Models.Add(model);
                await collection.InsertOneAsync(history);
            }           
        }
    }
}
