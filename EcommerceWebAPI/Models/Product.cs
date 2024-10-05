using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace EcommerceWebAPI.Models
{
    public class Product
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("productName")]
        public required string ProductName { get; set; }

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("category")]
        public string? Category { get; set; }

        [BsonElement("vendorId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string VendorId { get; set; }

        [BsonElement("stock")]
        public int Stock { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; }

        [BsonElement("rating")]
        public double Rating { get; set; }

        [BsonElement("base64Image")]
        public string? Base64Image { get; set; }
    }
}