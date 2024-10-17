using Microsoft.AspNetCore.Mvc;
using EcommerceWebAPI.Models;
using MongoDB.Driver;
using EcommerceWebAPI.Data;

namespace EcommerceWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IMongoCollection<Product> _products;

        public ProductController(MongoDBContext dbContext)
        {
            _products = dbContext.GetCollection<Product>("products");
        }

        [HttpGet]
        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            return await _products.Find(FilterDefinition<Product>.Empty).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product?>> GetProductById(string id)
        {
            var filter = Builders<Product>.Filter.Eq(x => x.Id, id);
            var product = _products.Find(filter).FirstOrDefault();
            return product is not null ? Ok(product) : NotFound();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProduct(Product updatedProduct)
        {
            var filter = Builders<Product>.Filter.Eq(x => x.Id, updatedProduct.Id);
            await _products.ReplaceOneAsync(filter, updatedProduct);
            return Ok(updatedProduct);
        }

        [HttpPost]
        public async Task<ActionResult> CreateProduct(Product newProduct)
        {
            await _products.InsertOneAsync(newProduct);
            return CreatedAtAction(nameof(GetProductById), new { id = newProduct.Id }, newProduct);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(string id)
        {
            var filter = Builders<Product>.Filter.Eq(x => x.Id, id);
            await _products.DeleteOneAsync(filter);
            return Ok();
        }

        // Activate a product
        [HttpPatch("activate/{id}")]
        public async Task<IActionResult> ActivateProduct(string id)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
            var update = Builders<Product>.Update.Set(p => p.IsActive, true);

            var result = await _products.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0)
            {
                return Ok(new { message = "Product activated successfully." });
            }

            return NotFound(new { message = "Product not found." });
        }

        // Deactivate a product
        [HttpPatch("deactivate/{id}")]
        public async Task<IActionResult> DeactivateProduct(string id)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
            var update = Builders<Product>.Update.Set(p => p.IsActive, false);

            var result = await _products.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0)
            {
                return Ok(new { message = "Product deactivated successfully." });
            }

            return NotFound(new { message = "Product not found." });
        }

        // Get products by VendorId
        [HttpGet("vendor/{vendorId}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByVendorId(string vendorId)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.VendorId, vendorId);
            var products = await _products.Find(filter).ToListAsync();

            if (products.Count > 0)
            {
                return Ok(products);
            }

            return NotFound(new { message = "No products found for this vendor." });
        }
    }
}
