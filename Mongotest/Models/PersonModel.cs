using MongoDB.Bson.Serialization.Attributes;

namespace Mongotest.Models
{
    public class PersonModel : BaseModel
    {

        public string Name { get; set; }
        public string Age { get; set; }
    }
}
