namespace Mongotest.Models
{
    public class HistoryModel<T> : BaseModel
    {
        public string? Notes { get; set; }
        public string ModelId { get; set; } = string.Empty;
        public T? Model { get; set; }
    }
}
