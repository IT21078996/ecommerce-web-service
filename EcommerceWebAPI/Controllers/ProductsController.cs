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
            return Ok();
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
    }
}
