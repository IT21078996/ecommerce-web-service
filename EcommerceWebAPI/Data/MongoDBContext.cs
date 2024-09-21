using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EcommerceWebAPI.Data
{
    public class MongoDBContext
    {
        private readonly IMongoDatabase _database;

        public MongoDBContext(IOptions<MongoDBSettings> settings)
        {
            // Create MongoClientSettings from the connection string
            var mongoSettings = MongoClientSettings.FromConnectionString(settings.Value.ConnectionString);

            // Set the ServerApi to V1 (as per MongoDB's suggestion)
            mongoSettings.ServerApi = new ServerApi(ServerApiVersion.V1);

            // Create a new MongoClient with the settings
            var client = new MongoClient(mongoSettings);

            // Assign the database
            _database = client.GetDatabase(settings.Value.Database);

            // Test the connection by pinging MongoDB
            try
            {
                var result = _database.RunCommand<BsonDocument>(new BsonDocument("ping", 1));
                Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to connect to MongoDB: " + ex.Message);
            }
        }

        // Method to get the MongoDB collection for a specific type
        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }
    }
}
