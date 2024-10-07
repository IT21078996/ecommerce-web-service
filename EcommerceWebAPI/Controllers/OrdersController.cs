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

        public OrderController(MongoDBContext dbContext)
        {
            _orders = dbContext.GetCollection<Order>("orders");
        }

        [HttpGet]
        public async Task<IEnumerable<Order>> GetAllOrders()
        {
            return await _orders.Find(FilterDefinition<Order>.Empty).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Inventory?>> GetOrderById(string id)
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Id, id);
            var order = _orders.Find(filter).FirstOrDefault();
            return order is not null ? Ok(order) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrder(Order newOrder)
        {
            await _orders.InsertOneAsync(newOrder);
            return CreatedAtAction(nameof(GetOrderById), new { id = newOrder.Id }, newOrder);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrder(Order updatedOrder)
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Id, updatedOrder.Id);
            await _orders.ReplaceOneAsync(filter, updatedOrder);
            return Ok(updatedOrder);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrder(string id)
        {
            var filter = Builders<Order>.Filter.Eq(x => x.Id, id);
            await _orders.DeleteOneAsync(filter);
            return Ok();
        }

        [HttpGet("user/{customerId}")]
        public async Task<IEnumerable<Order>> GetOrdersByCustomerId(string customerId)
        {
            var objectId = new ObjectId(customerId);  
            var filter = Builders<Order>.Filter.Eq("customerId", objectId); 
            return await _orders.Find(filter).ToListAsync();
        }



    }
}
