using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using EcommerceWebAPI.Models;
using EcommerceWebAPI.Data;

namespace EcommerceWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorController : ControllerBase
    {
        private readonly IMongoCollection<Vendor> _vendors;

        public VendorController(MongoDBContext dbContext)
        {
            _vendors = dbContext.GetCollection<Vendor>("vendors");
        }

        [HttpGet]
        public async Task<IEnumerable<Vendor>> Get()
        {
            return await _vendors.Find(FilterDefinition<Vendor>.Empty).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Vendor?>> GetById(string id)
        {
            var filter = Builders<Vendor>.Filter.Eq(x => x.Id, id);
            var vendor = _vendors.Find(filter).FirstOrDefault();
            return vendor is not null ? Ok(vendor) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult> Create(Vendor newVendor)
        {
            await _vendors.InsertOneAsync(newVendor);
            return CreatedAtAction(nameof(GetById), new { id = newVendor.Id }, newVendor);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Vendor vendor)
        {
            var filter = Builders<Vendor>.Filter.Eq(x => x.Id, vendor.Id);
            await _vendors.ReplaceOneAsync(filter, vendor);
            return Ok(vendor);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteVendor(string id)
        {
            var filter = Builders<Vendor>.Filter.Eq(x => x.Id, id);
            await _vendors.DeleteOneAsync(filter);
            return Ok();
        }
    }
}
