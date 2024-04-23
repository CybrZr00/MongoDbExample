namespace Shared.Models
{
    public class PersonModel : BaseModel
    {
        public string Name { get; set; } = "Default Name";
        public int Age { get; set; } = 0;

        public bool IsHuman { get; set; }
    }
    public class PersonModelEf : BaseModelEf
    {
        public string Name { get; set; } = "Default Name";
        public int Age { get; set; } = 0;

        public bool IsHuman { get; set; }
    }
    public class PersonModelCreate
    {
        public string Name { get; set; } = "Default Name";
        public int Age { get; set; } = 0;

        public bool IsHuman { get; set; }
    }
    public class PersonModelEdit
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "Default Name";
        public int Age { get; set; } = 0;

        public bool IsHuman { get; set; }
    }
}
