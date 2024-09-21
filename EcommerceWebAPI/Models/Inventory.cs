using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EcommerceWebAPI.Models
{
    public class Inventory
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("ProductId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ProductId { get; set; }

        [BsonElement("Quantity")]
        public int Quantity { get; set; }

        [BsonElement("LastRestockDate")]
        public DateTime LastRestockDate { get; set; }

        [BsonElement("RestockQuantity")]
        public int RestockQuantity { get; set; }

        [BsonElement("IsLowStock")]
        public bool IsLowStock { get; set; }
    }
}
