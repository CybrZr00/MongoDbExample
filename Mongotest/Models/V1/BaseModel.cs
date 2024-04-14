using MongoDB.Bson.Serialization.Attributes;

using System.ComponentModel.DataAnnotations;

namespace Mongotest.Models.V1
{
    public class BaseModel
    {
        [Key]
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)] // This is used to convert the string ID into an ObjectId
        public string Id { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:U}", ApplyFormatInEditMode = true)]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:U}", ApplyFormatInEditMode = true)]
        public DateTime DateLastUpdated { get; set; } = DateTime.UtcNow;
    }
    //public class BaseModelEf
    //{
    //    [Key]
    //    public Guid Id { get; set; }
    //    [DataType(DataType.DateTime)]
    //    [DisplayFormat(DataFormatString = "{0:U}", ApplyFormatInEditMode = true)]
    //    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    //    [DataType(DataType.DateTime)]
    //    [DisplayFormat(DataFormatString = "{0:U}", ApplyFormatInEditMode = true)]
    //    public DateTime DateLastUpdated { get; set; } = DateTime.UtcNow;
    //}
}
