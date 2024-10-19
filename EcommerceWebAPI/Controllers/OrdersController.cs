using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using EcommerceWebAPI.Models;
using EcommerceWebAPI.Data;
using MongoDB.Bson;

namespace EcommerceWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMongoCollection<Order> _orders;
        private readonly IMongoCollection<Product> _products;

        public OrderController(MongoDBContext dbContext)
        {
            _orders = dbContext.GetCollection<Order>("orders");
            _products = dbContext.GetCollection<Product>("products");
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
            // Begin transaction or lock resources if necessary for consistency
            var session = await _orders.Database.Client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                // Loop through each product in the order and reduce the stock
                foreach (var orderProduct in newOrder.Products)
                {
                    var filter = Builders<Product>.Filter.Eq(p => p.Id, orderProduct.ProductId);
                    var product = await _products.Find(filter).FirstOrDefaultAsync();

                    if (product == null)
                    {
                        await session.AbortTransactionAsync();
                        return NotFound(new { message = $"Item {orderProduct.ProductName} not found" });
                    }

                    if (product.Stock < orderProduct.Quantity)
                    {
                        await session.AbortTransactionAsync();
                        return BadRequest(new { message = $"Not enough stock for item {product.ProductName}" });
                    }

                    product.Stock -= orderProduct.Quantity;

                    await _products.ReplaceOneAsync(filter, product);
                }

                await _orders.InsertOneAsync(newOrder);

                await session.CommitTransactionAsync();

                return CreatedAtAction(nameof(GetOrderById), new { id = newOrder.Id }, newOrder);
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                return StatusCode(500, new { message = "An error occurred while processing the order", error = ex.Message });
            }
            finally
            {
                session.Dispose();
            }
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
            var session = await _orders.Database.Client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                var order = await _orders.Find(o => o.Id == id).FirstOrDefaultAsync();
                if (order == null)
                {
                    await session.AbortTransactionAsync();
                    return NotFound(new { message = "Order not found" });
                }

                // Set status, overriding readiness checks
                if (request.Status == OrderStatus.OrderDispatched || request.Status == OrderStatus.Delivered)
                {
                    order.Status = request.Status;
                }
                else
                {
                    // Check if all products for the specific vendor are marked as ready
                    var vendorProductsReady = true;
                    foreach (var product in order.Products.Where(p => p.VendorId == request.VendorId))
                    {
                        if (!product.IsReady)
                        {
                            vendorProductsReady = false;
                            break;
                        }
                    }

                    // Update order status to 'Partially Ready' if one vendor has completed their products
                    if (vendorProductsReady && order.Status == OrderStatus.Processing)
                    {
                        order.Status = OrderStatus.PartiallyReady;
                    }

                    // Check if all products from all vendors are marked as ready
                    var allProductsReady = order.Products.All(p => p.IsReady);
                    if (allProductsReady && order.Status != OrderStatus.OrderDispatched)
                    {
                        order.Status = OrderStatus.ReadyForShipment;
                    }
                }

                await _orders.ReplaceOneAsync(o => o.Id == id, order);

                await session.CommitTransactionAsync();

                return Ok(new { message = "Order status updated", status = order.Status });
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                return StatusCode(500, new { message = "An error occurred while updating the order status", error = ex.Message });
            }
            finally
            {
                session.Dispose();
            }
        }

        // Cancel an order with a cancellation note
        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(string id, [FromBody] CancelOrderRequest request)
        {
            var session = await _orders.Database.Client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                var order = await _orders.Find(o => o.Id == id).FirstOrDefaultAsync();
                if (order == null)
                {
                    await session.AbortTransactionAsync();
                    return NotFound(new { message = "Order not found" });
                }

                if (order.Status == OrderStatus.Cancelled)
                {
                    await session.AbortTransactionAsync();
                    return BadRequest(new { message = "Order is already cancelled" });
                }

                order.Status = OrderStatus.Cancelled;
                order.CancellationNote = request.CancellationNote;

                // Add quantities of the products back to the inventory
                foreach (var orderProduct in order.Products)
                {
                    var filter = Builders<Product>.Filter.Eq(p => p.Id, orderProduct.ProductId);
                    var product = await _products.Find(filter).FirstOrDefaultAsync();

                    if (product == null)
                    {
                        await session.AbortTransactionAsync();
                        return NotFound(new { message = $"Product {orderProduct.ProductName} not found" });
                    }

                    product.Stock += orderProduct.Quantity;

                    await _products.ReplaceOneAsync(filter, product);
                }

                await _orders.ReplaceOneAsync(o => o.Id == id, order);

                await session.CommitTransactionAsync();

                return Ok(new { message = "Order cancelled and inventory updated", order });
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                return StatusCode(500, new { message = "An error occurred while cancelling the order", error = ex.Message });
            }
            finally
            {
                session.Dispose();
            }
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
        
        [HttpGet("user/{customerId}")]
        public async Task<IEnumerable<Order>> GetOrdersByCustomerId(string customerId)
        {
            var objectId = new ObjectId(customerId);  
            var filter = Builders<Order>.Filter.Eq("customerId", objectId); 
            return await _orders.Find(filter).ToListAsync();
        }

    }

    // Request model for updating order status
    public class UpdateOrderStatusRequest
    {
        public OrderStatus Status { get; set; }
        public string? VendorId { get; set; }
    }

    // Request model for cancelling an order
    public class CancelOrderRequest
    {
        public string? CancellationNote { get; set; }
    }
}
