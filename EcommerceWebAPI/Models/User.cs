using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EcommerceWebAPI.Models
{
    public class User
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("email")]
        public required string Email { get; set; }

        [BsonElement("username")]
        public required string UserName { get; set; }

        [BsonElement("password")]
        public required string Password { get; set; }

        [BsonElement("role")]
        public required string Role { get; set; } // Administrator, Vendor, CSR

        [BsonElement("isActive")]
        public bool IsActive { get; set; }
    }
}
