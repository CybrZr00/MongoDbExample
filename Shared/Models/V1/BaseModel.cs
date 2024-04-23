using MongoDB.Bson.Serialization.Attributes;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Shared.Models
{
    public class BaseModel
    {
        private string id;

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        [JsonIgnore]// This is used to convert the string ID into an ObjectId
        public string Id { get => id; set { id = value; SID = value.ToString(); } }
        [BsonIgnore]
        [NotMapped] public string Name { get; set; }
        public string SID { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:U}", ApplyFormatInEditMode = true)]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:U}", ApplyFormatInEditMode = true)]
        public DateTime DateLastUpdated { get; set; } = DateTime.UtcNow;
    }
    public class BaseModelEf
    {
        [Key]
        public Guid Id { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:U}", ApplyFormatInEditMode = true)]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:U}", ApplyFormatInEditMode = true)]
        public DateTime DateLastUpdated { get; set; } = DateTime.UtcNow;
    }
}
