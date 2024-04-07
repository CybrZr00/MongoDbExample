using MongoDB.Bson;
using MongoDB.Driver;

using Mongotest.Models;

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
            var type = typeof(T);
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
            var history = new HistoryModel<T>
            {
                Id = id,
                ModelId = id,
                Model = model,
                Notes = notes,
                DateLastUpdated = DateTime.Now
            };
            var collectionString = $"HistoryModel<{collectionName}>";
            var collection = ConnectDb<HistoryModel<T>>(collectionString);

            await collection.ReplaceOneAsync(f => f.Model != null && f.Model.Id == id, history, new ReplaceOptions { IsUpsert = true });
        }
    }
}
