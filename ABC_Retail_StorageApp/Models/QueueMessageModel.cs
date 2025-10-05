namespace ABC_Retail_StorageApp.Models
{
    public class QueueMessageModel
    {
        public string MessageId { get; set; } = string.Empty;
        public string MessageText { get; set; } = string.Empty;
        public DateTimeOffset? InsertedOn { get; set; }
    }
}
