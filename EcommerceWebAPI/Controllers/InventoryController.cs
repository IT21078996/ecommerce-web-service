using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using EcommerceWebAPI.Models;
using EcommerceWebAPI.Data;

namespace EcommerceWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IMongoCollection<Inventory> _inventory;

        public InventoryController(MongoDBContext dbContext)
        {
            _inventory = dbContext.GetCollection<Inventory>("inventory");
        }

        [HttpGet]
        public async Task<IEnumerable<Inventory>> GetAllInventory()
        {
            return await _inventory.Find(FilterDefinition<Inventory>.Empty).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Inventory?>> GetInventoryById(string id)
        {
            var filter = Builders<Inventory>.Filter.Eq(x => x.Id, id);
            var inventory = _inventory.Find(filter).FirstOrDefault();
            return inventory is not null ? Ok(inventory) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult> AddInventoryItem(Inventory newItem)
        {
            await _inventory.InsertOneAsync(newItem);
            return CreatedAtAction(nameof(GetInventoryById), new { id = newItem.Id }, newItem);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateInventoryItem(Inventory updatedItem)
        {
            var filter = Builders<Inventory>.Filter.Eq(x => x.Id, updatedItem.Id);
            await _inventory.ReplaceOneAsync(filter, updatedItem);
            return Ok(updatedItem);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteInventoryItem(string id)
        {
            var filter = Builders<Inventory>.Filter.Eq(x => x.Id, id);
            await _inventory.DeleteOneAsync(filter);
            return Ok();
        }
    }
}
