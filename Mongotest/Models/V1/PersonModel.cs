namespace Mongotest.Models.V1
{
    public class PersonModel : BaseModel
    {
        public string Name { get; set; } = "Default Name";
        public int Age { get; set; } = 0;

        public bool IsHuman { get; set; }
    }
    //public class PersonModelEf : BaseModelEf
    //{
    //    public string Name { get; set; } = "Default Name";
    //    public int Age { get; set; } = 0;

    //    public bool IsHuman { get; set; }
    //}
}
