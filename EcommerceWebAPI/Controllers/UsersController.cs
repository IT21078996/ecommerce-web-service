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
        private readonly IMongoCollection<Vendor> _vendors;

        public UserController(MongoDBContext dbContext)
        {
            _users = dbContext.GetCollection<User>("users");
            _vendors = dbContext.GetCollection<Vendor>("vendors");
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

            if (newUser.Role == "Vendor")
            {
                // Create a new Vendor using the same Id from the User
                var newVendor = new Vendor
                {
                    Id = newUser.Id,
                    VendorName = newUser.Username ?? "Vendor Name",
                    Email = newUser.Email,
                    IsActive = newUser.IsActive,
                    Phone = newUser.ContactNumber,
                    Rating = 0,
                    ReviewCount = 0,
                    Products = new List<string>(),
                    Address = ""
                };

                await _vendors.InsertOneAsync(newVendor);
            }

            return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(User updatedUser)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, updatedUser.Id);
            await _users.ReplaceOneAsync(filter, updatedUser);

            // If the updated user is a Vendor, update corresponding vendor information
            if (updatedUser.Role == "Vendor")
            {
                var vendorFilter = Builders<Vendor>.Filter.Eq(x => x.Id, updatedUser.Id);
                var vendorUpdate = Builders<Vendor>.Update
                    .Set(x => x.VendorName, updatedUser.Username)
                    .Set(x => x.Email, updatedUser.Email)
                    .Set(x => x.Phone, updatedUser.ContactNumber);

                await _vendors.UpdateOneAsync(vendorFilter, vendorUpdate);
            }

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

        [HttpPut("updateProfile/{id}")]
        public async Task<ActionResult> UpdateUserProfile(string id, [FromBody] User updatedUser)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var existingUser = await _users.Find(filter).FirstOrDefaultAsync();

            if (existingUser == null)
            {
                return NotFound("User not found");
            }

            var update = Builders<User>.Update
                .Set(x => x.Username, updatedUser.Username);

            if (updatedUser.ProfilePictureBase64 != null)
            {
                update = update.Set(x => x.ProfilePictureBase64, updatedUser.ProfilePictureBase64);
            }

            if (updatedUser.ContactNumber != null)
            {
                update = update.Set(x => x.ContactNumber, updatedUser.ContactNumber);
            }

            await _users.UpdateOneAsync(filter, update);
            return Ok();
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<User?>> GetUserByEmail(string email)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Email, email);
            var user = _users.Find(filter).FirstOrDefault();
            return user is not null ? Ok(user) : NotFound();
        }
    }
}
