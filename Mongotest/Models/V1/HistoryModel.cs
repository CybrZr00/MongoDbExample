using MongoDB.Bson.Serialization.Attributes;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Mongotest.Models.V1
{
    public class HistoryModel<T> : BaseModel
    {
        public string? Notes { get; set; }
        public Guid ModelId { get; set; }
        public List<T>? Models { get; set; } = new List<T>();
    }

    public class HistoryModelEF : BaseModel
    {
        public string? Notes { get; set; }
        public Guid ModelId { get; set; }
        [BsonIgnore]
        [NotMapped]
        public List<HistoryItem> HistoryEntries { get; set; } = new List<HistoryItem>();
    }

    public class HistoryItem
    {
        private string modelJson = "{\"error\":\"Invalid JSON\"}";
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ModelType { get; set; } = string.Empty;
        public string ModelJson { get => modelJson; 
            set
            {
                try
                {
                    JsonDocument.Parse(value);
                    modelJson = value;
                }
                catch (JsonException)
                {
                    //throw new ArgumentException("Invalid JSON format");
                    modelJson = "{\"error\":\"Invalid JSON\"}";
                }
            } 
        }
        public Guid HistoryModelEFId { get; set; }
        public HistoryAction Action { get; set; } = HistoryAction.Created;
    }
    public enum HistoryAction
    {
        Created,
        Updated,
        Deleted
    }
}
