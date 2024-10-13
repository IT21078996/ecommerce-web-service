using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using EcommerceWebAPI.Models;
using EcommerceWebAPI.Data;

namespace EcommerceWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMongoCollection<Order> _orders;

        public OrderController(MongoDBContext dbContext)
        {
            _orders = dbContext.GetCollection<Order>("orders");
        }

        // Get all orders
        [HttpGet]
        public async Task<IEnumerable<Order>> GetAllOrders()
        {
            return await _orders.Find(FilterDefinition<Order>.Empty).ToListAsync();
        }

        // Get order by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Order?>> GetOrderById(string id)
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Id, id);
            var order = _orders.Find(filter).FirstOrDefault();
            return order is not null ? Ok(order) : NotFound();
        }

        // Create a new order
        [HttpPost]
        public async Task<ActionResult> CreateOrder(Order newOrder)
        {
            await _orders.InsertOneAsync(newOrder);
            return CreatedAtAction(nameof(GetOrderById), new { id = newOrder.Id }, newOrder);
        }

        // Update order
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrder(Order updatedOrder)
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Id, updatedOrder.Id);
            await _orders.ReplaceOneAsync(filter, updatedOrder);
            return Ok(updatedOrder);
        }

        // Update order status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateOrderStatusRequest request)
        {
            var order = await _orders.Find(o => o.Id == id).FirstOrDefaultAsync();
            if (order == null)
                return NotFound(new { message = "Order not found" });

            // Update order status
            order.Status = request.Status;

            // Handle multi-vendor readiness check for "Partially Ready" or "ReadyForShipment"
            //if (request.Status == OrderStatus.ReadyForShipment && order.Products.Any(p => !p.IsReady))
            //{
            //    order.Status = OrderStatus.PartiallyReady;
            //}

            await _orders.ReplaceOneAsync(o => o.Id == id, order);

            return Ok(new { message = "Order status updated", status = order.Status });
        }

        // Cancel an order with a cancellation note
        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(string id, [FromBody] CancelOrderRequest request)
        {
            var order = await _orders.Find(o => o.Id == id).FirstOrDefaultAsync();
            if (order == null)
                return NotFound(new { message = "Order not found" });

            // Update order status to Cancelled and add cancellation note
            order.Status = OrderStatus.Cancelled;
            order.CancellationNote = request.CancellationNote;

            await _orders.ReplaceOneAsync(o => o.Id == id, order);

            return Ok(new { message = "Order cancelled", order });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrder(string id)
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Id, id);
            await _orders.DeleteOneAsync(filter);
            return Ok();
        }

        [HttpGet("vendor/{vendorId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByVendor(string vendorId)
        {
            // Filter orders that contain at least one product from the specified vendor
            var filter = Builders<Order>.Filter.ElemMatch(o => o.Products, p => p.VendorId == vendorId);

            var orders = await _orders.Find(filter).ToListAsync();

            if (orders.Count == 0)
            {
                return NotFound(new { message = "No orders found for this vendor" });
            }

            return Ok(orders);
        }

        [HttpGet("pendingOrders/{productId}")]
        public async Task<ActionResult<bool>> CheckPendingOrdersForProduct(string productId)
        {
            // Define filter to check for orders containing the product with the specified ID and a pending status
            var filter = Builders<Order>.Filter.And(
                Builders<Order>.Filter.ElemMatch(o => o.Products, p => p.ProductId == productId),
                Builders<Order>.Filter.Eq(o => o.Status, OrderStatus.Pending)
            );

            var hasPendingOrders = await _orders.Find(filter).AnyAsync();

            return Ok(hasPendingOrders);
        }

    }

    // Request model for updating order status
    public class UpdateOrderStatusRequest
    {
        public OrderStatus Status { get; set; }
    }

    // Request model for cancelling an order
    public class CancelOrderRequest
    {
        public string? CancellationNote { get; set; }
    }
}
