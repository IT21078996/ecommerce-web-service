using EcommerceWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace EcommerceWebAPI.Models
{
    public enum OrderStatus
    {
        Pending,
        Processing,
        ReadyForShipment,
        //PartiallyReady,
        OrderDispatched,
        //PartiallyDelivered,
        Delivered,
        Cancelled
    }

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
        public List<OrderProduct>? Products { get; set; }

        [BsonElement("totalAmount")]
        public decimal TotalAmount { get; set; }

        [BsonElement("status")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [BsonElement("isCancelled")]
        public bool IsCancelled { get; set; }

        [BsonElement("shippingAddress")]
        public required string ShippingAddress { get; set; }

        [BsonElement("cancellationNote")]
        public string? CancellationNote { get; set; }
    }

    public class OrderProduct
    {
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string ProductId { get; set; }

        [BsonElement("productName")]
        public string? ProductName { get; set; }

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("vendorId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string VendorId { get; set; }

        [BsonElement("isReady")]
        public bool IsReady { get; set; } = false;  // For multi-vendor orders, track readiness per vendor
    }
}
