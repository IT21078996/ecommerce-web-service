using EcommerceWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace EcommerceWebAPI.Models
{
    public class Order
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("orderDate")]
        public DateTime OrderDate { get; set; }

        [BsonElement("customerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string CustomerId { get; set; }

        [BsonElement("products")]
        public List<Product>? Products { get; set; }

        [BsonElement("totalAmount")]
        public decimal TotalAmount { get; set; }

        [BsonElement("status")]
        public string? Status { get; set; }

        [BsonElement("isCancelled")]
        public bool IsCancelled { get; set; }

        [BsonElement("shippingAddress")]
        public required string ShippingAddress { get; set; }

        [BsonElement("vendorId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string VendorId { get; set; }
    }
}
