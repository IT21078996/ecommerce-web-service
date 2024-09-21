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

        //[HttpGet]
        //public IActionResult GetAllVendors()
        //{
        //    var vendors = _vendors.Find(vendor => true).ToList();
        //    return Ok(vendors);
        //}
        [HttpGet]
        public async Task<IEnumerable<Vendor>> Get()
        {
            return await _vendors.Find(FilterDefinition<Vendor>.Empty).ToListAsync();
        }

        //[HttpGet("{id}")]
        //public IActionResult GetVendorById(string id)
        //{
        //    // Parse the ID from the string to ObjectId
        //    if (!ObjectId.TryParse(id, out ObjectId objectId))
        //    {
        //        return BadRequest("Invalid ID format.");
        //    }

        //    var vendor = _vendors.Find(v => v.Id == objectId).FirstOrDefault();
        //    if (vendor == null) return NotFound();
        //    return Ok(vendor);
        //}

        [HttpGet("{id}")]
        public async Task<ActionResult<Vendor?>> GetById(string id)
        {
            var filter = Builders<Vendor>.Filter.Eq(x => x.Id, id);
            var vendor = _vendors.Find(filter).FirstOrDefault();
            return vendor is not null ? Ok(vendor) : NotFound();
        }

        [HttpPost]
        //public IActionResult CreateVendor([FromBody] Vendor newVendor)
        //{
        //    _vendors.InsertOne(newVendor);
        //    return Ok(newVendor);
        //}

        public async Task<ActionResult> Create(Vendor newVendor)
        {
            await _vendors.InsertOneAsync(newVendor);
            return CreatedAtAction(nameof(GetById), new { id = newVendor.Id }, newVendor);
        }

        [HttpPut("{id}")]
        //public IActionResult UpdateVendor(string id, [FromBody] Vendor updatedVendor)
        //{
        //    // Parse the ID from the string to ObjectId
        //    if (!ObjectId.TryParse(id, out ObjectId objectId))
        //    {
        //        return BadRequest("Invalid ID format.");
        //    }

        //    // Fetch the existing vendor to ensure the ID is maintained
        //    var existingVendor = _vendors.Find(v => v.Id == objectId).FirstOrDefault();
        //    if (existingVendor == null) return NotFound();

        //    // Ensure that the updatedVendor's ID is the same as the existing vendor
        //    updatedVendor.Id = existingVendor.Id;

        //    // Perform the update
        //    var result = _vendors.ReplaceOne(v => v.Id == objectId, updatedVendor);
        //    if (result.IsAcknowledged) return Ok(updatedVendor);

        //    return NotFound();
        //}

        public async Task<ActionResult> Update(Vendor vendor)
        {
            var filter = Builders<Vendor>.Filter.Eq(x => x.Id, vendor.Id);
            await _vendors.ReplaceOneAsync(filter, vendor);
            return Ok();
        }

        [HttpDelete("{id}")]
        //public IActionResult DeleteVendor(string id)
        //{
        //    // Parse the ID from the string to ObjectId
        //    if (!ObjectId.TryParse(id, out ObjectId objectId))
        //    {
        //        return BadRequest("Invalid ID format.");
        //    }

        //    var result = _vendors.DeleteOne(v => v.Id == objectId);
        //    if (result.DeletedCount > 0) return Ok();
        //    return NotFound();
        //}
        public async Task<ActionResult> DeleteVendor(string id)
        {
            var filter = Builders<Vendor>.Filter.Eq(x => x.Id, id);
            await _vendors.DeleteOneAsync(filter);
            return Ok();
        }
    }
}
