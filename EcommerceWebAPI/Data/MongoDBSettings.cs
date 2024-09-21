namespace EcommerceWebAPI.Data
{
    public class MongoDBSettings
    {
        public required string ConnectionString { get; set; }
        public required string Database { get; set; }
    }
}
