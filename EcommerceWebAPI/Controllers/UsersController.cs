using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using EcommerceWebAPI.Models;
using EcommerceWebAPI.Data;

namespace EcommerceWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMongoCollection<User> _users;

        public UserController(MongoDBContext dbContext)
        {
            _users = dbContext.GetCollection<User>("users");
        }

        [HttpGet]
        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _users.Find(FilterDefinition<User>.Empty).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User?>> GetUserById(string id)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var user = _users.Find(filter).FirstOrDefault();
            return user is not null ? Ok(user) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult> CreateUser(User newUser)
        {
            await _users.InsertOneAsync(newUser);
            return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(User updatedUser)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, updatedUser.Id);
            await _users.ReplaceOneAsync(filter, updatedUser);
            return Ok(updatedUser);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            await _users.DeleteOneAsync(filter);
            return Ok();
        }

        [HttpGet("pending")]
        public async Task<IEnumerable<User>> GetPendingUsers()
        {
            var filter = Builders<User>.Filter.Eq(x => x.IsActive, false);
            return await _users.Find(filter).ToListAsync();
        }

        [HttpPut("approve/{id}")]
        public async Task<ActionResult> ApproveUser(string id)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var update = Builders<User>.Update.Set(x => x.IsActive, true);
            await _users.UpdateOneAsync(filter, update);
            return Ok();
        }

    }
}
