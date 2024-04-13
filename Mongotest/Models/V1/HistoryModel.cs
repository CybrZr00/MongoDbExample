using MongoDB.Bson.Serialization.Attributes;

using System.ComponentModel.DataAnnotations;

namespace Mongotest.Models.V1
{
    public class HistoryModel<T> : BaseModel
    {
        public string? Notes { get; set; }
        public string ModelId { get; set; } = string.Empty;
        public List<T>? Models { get; set; } = new List<T>();
    }
}
