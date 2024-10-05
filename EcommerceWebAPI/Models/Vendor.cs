using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace EcommerceWebAPI.Models
{
    public class Vendor
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("vendorName")]
        public required string VendorName { get; set; }

        [BsonElement("email")]
        public string? Email { get; set; }

        [BsonElement("phone")]
        public string? Phone { get; set; }

        [BsonElement("rating")]
        public double Rating { get; set; }

        [BsonElement("reviewCount")]
        public int ReviewCount { get; set; }

        [BsonElement("address")]
        public string? Address { get; set; }

        [BsonElement("products")]
        public List<string>? Products { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; }
    }
}
