using MongoDB.Bson.Serialization.Attributes;

namespace Mongotest.Models.V1
{
    public class BaseModel
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)] // This is used to convert the string ID into an ObjectId
        public string Id { get; set; } = "";
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateLastUpdated { get; set; } = DateTime.UtcNow;
    }
}
