namespace ABC_Retail_StorageApp.Models
{
    public class TableEntityModel
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}
