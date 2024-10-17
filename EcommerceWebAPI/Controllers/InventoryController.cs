using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using EcommerceWebAPI.Models;
using EcommerceWebAPI.Data;
using System.Security.Claims;

namespace EcommerceWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IMongoCollection<Inventory> _inventory;
        private readonly IMongoCollection<Notification> _notifications;
        private readonly IMongoCollection<Product> _productsCollection;

        public InventoryController(MongoDBContext dbContext)
        {
            _inventory = dbContext.GetCollection<Inventory>("inventory");
            _notifications = dbContext.GetCollection<Notification>("notifications");
            _productsCollection = dbContext.GetCollection<Product>("products");
        }

        [HttpGet]
        public async Task<IEnumerable<Inventory>> GetAllInventory()
        {
            return await _inventory.Find(FilterDefinition<Inventory>.Empty).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult> AddInventoryItem(Inventory newItem)
        {
            await _inventory.InsertOneAsync(newItem);
            return CreatedAtAction(nameof(GetInventoryById), new { id = newItem.Id }, newItem);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Inventory?>> GetInventoryById(string id)
        {
            var filter = Builders<Inventory>.Filter.Eq(x => x.Id, id);
            var inventory = _inventory.Find(filter).FirstOrDefault();
            return inventory is not null ? Ok(inventory) : NotFound();
        }

        // Update inventory item and send low stock notification if necessary
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateInventoryItem(string id, Inventory updatedItem)
        {
            var filter = Builders<Inventory>.Filter.Eq(x => x.Id, updatedItem.Id);
            await _inventory.ReplaceOneAsync(filter, updatedItem);

            // Check for low stock and send notification
            if (updatedItem.Quantity < 5)
            {
                var notification = new Notification
                {
                    VendorId = updatedItem.VendorId,
                    Message = $"Low stock alert: Product {updatedItem.ProductId} has only {updatedItem.Quantity} units left.",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notifications.InsertOneAsync(notification);  // Insert into the notifications collection
            }

            return Ok(updatedItem);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteInventoryItem(string id)
        {
            var filter = Builders<Inventory>.Filter.Eq(x => x.Id, id);
            await _inventory.DeleteOneAsync(filter);
            return Ok();
        }

        [HttpGet("vendor/{vendorId}")]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetInventoryByVendorId(string vendorId)
        {
            // Ensure vendorId is valid
            if (string.IsNullOrEmpty(vendorId))
            {
                return BadRequest("Vendor ID is required");
            }

            var filter = Builders<Inventory>.Filter.Eq(i => i.VendorId, vendorId);
            var inventoryItems = await _inventory.Find(filter).ToListAsync();

            if (inventoryItems == null || !inventoryItems.Any())
            {
                return NotFound($"No inventory found for Vendor ID: {vendorId}");
            }

            return Ok(inventoryItems);
        }

        [HttpGet("lowStock")]
        public async Task<IEnumerable<Inventory>> GetLowStockItems()
        {
            var vendorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine("Extracted Vendor ID: " + vendorId); // Check the vendor ID

            if (string.IsNullOrEmpty(vendorId))
            {
                // If vendorId is not found, return an empty list or some error response
                return new List<Inventory>();
            }

            var filter = Builders<Inventory>.Filter.And(
                Builders<Inventory>.Filter.Eq(i => i.VendorId, vendorId),
                Builders<Inventory>.Filter.Lt(i => i.Quantity, 5)
            );

            return await _inventory.Find(filter).ToListAsync();
        }

        // Get inventory items with low stock for a specific vendor (threshold < 5)
        [HttpGet("lowStock/vendor/{vendorId}")]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetLowStockItemsByVendorId(string vendorId)
        {
            if (string.IsNullOrEmpty(vendorId))
            {
                return BadRequest("Vendor ID is required");
            }

            var filter = Builders<Inventory>.Filter.And(
                Builders<Inventory>.Filter.Eq(i => i.VendorId, vendorId),
                Builders<Inventory>.Filter.Lt(i => i.Quantity, 5) // Low stock threshold
            );

            var lowStockItems = await _inventory.Find(filter).ToListAsync();

            if (!lowStockItems.Any())
            {
                return NotFound($"No low stock items found for Vendor ID: {vendorId}");
            }

            return Ok(lowStockItems);
        }

        // In your ProductsController.cs or InventoryController.cs
        [HttpGet("products")]
        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            // Assuming you have a MongoDB collection for products
            var products = await _productsCollection.Find(FilterDefinition<Product>.Empty).ToListAsync();
            return products;
        }
    }
}
