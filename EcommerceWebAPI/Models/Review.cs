using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EcommerceWebAPI.Models
{
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string UserId { get; set; }  // ID of the user who left the review

        [BsonElement("vendorId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string VendorId { get; set; }  // ID of the vendor being reviewed

        [BsonElement("rating")]
        public int Rating { get; set; }  // Rating (e.g., 1 to 5 stars)

        [BsonElement("comment")]
        public required string Comment { get; set; }  // User comment

        [BsonElement("createdDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;  // Timestamp

        [BsonElement("updatedDate")]
        public DateTime? UpdatedDate { get; set; }  // Timestamp for updates
    }
}
