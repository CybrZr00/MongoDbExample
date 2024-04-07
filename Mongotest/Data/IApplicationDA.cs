using Mongotest.Models.V1;

namespace Mongotest.Data
{
    public interface IApplicationDA
    {
        Task<bool> Any<T>(string collectionName) where T : BaseModel;
        Task<T> CreateAsync<T>(T model, string collectionName) where T : BaseModel;
        Task DeleteAll<T>(string collectionName) where T : BaseModel;
        Task DeleteAsync<T>(string id, string collectionName) where T : BaseModel;
        Task<List<T>> FilterByField<T>(string field, bool exists, string collectionName) where T : BaseModel;
        Task<List<T>> FilterContains<T>(string field, string value, string collectionName) where T : BaseModel;
        Task<List<T>> FilterEquals<T>(string field, string value, string collectionName) where T : BaseModel;
        Task<List<T>> FilterGreaterThan<T>(string field, string value, string collectionName) where T : BaseModel;
        Task<List<T>> FilterLessThan<T>(string field, string value, string collectionName) where T : BaseModel;
        Task<List<T>> GetAllAsync<T>(string collectionName);
        Task<T> GetOneAsync<T>(string id, string collectionName) where T : BaseModel;
        Task<T> UpsertAsync<T>(string id, T model, string collectionName) where T : BaseModel;
    }
}