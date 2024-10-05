using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using EcommerceWebAPI.Models;
using EcommerceWebAPI.Data;

namespace EcommerceWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IMongoCollection<Review> _reviews;
        private readonly IMongoCollection<Vendor> _vendors;

        public ReviewController(MongoDBContext dbContext)
        {
            _reviews = dbContext.GetCollection<Review>("reviews");
            _vendors = dbContext.GetCollection<Vendor>("vendors");
        }

        // Add a new review for a vendor
        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] Review newReview)
        {
            // Insert the review
            await _reviews.InsertOneAsync(newReview);

            // Update vendor's average rating and review count
            await UpdateVendorRating(newReview.VendorId);

            return CreatedAtAction(nameof(GetReviewById), new { id = newReview.Id }, newReview);
        }

        // Get all reviews for a vendor
        [HttpGet("vendor/{vendorId}")]
        public async Task<IActionResult> GetReviewsByVendorId(string vendorId)
        {
            var reviews = await _reviews.Find(r => r.VendorId == vendorId).ToListAsync();
            return Ok(reviews);
        }

        // Get a review by review ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(string id)
        {
            var review = await _reviews.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (review == null)
                return NotFound();

            return Ok(review);
        }

        // Update a review
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(string id, [FromBody] Review updatedReview)
        {
            var review = await _reviews.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (review == null)
                return NotFound();

            // Update the review and set the update timestamp
            updatedReview.UpdatedDate = DateTime.UtcNow;
            await _reviews.ReplaceOneAsync(r => r.Id == id, updatedReview);

            // Recalculate the vendor's average rating
            await UpdateVendorRating(updatedReview.VendorId);

            return Ok(updatedReview);
        }

        // Delete a review
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(string id)
        {
            var review = await _reviews.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (review == null)
                return NotFound();

            // Delete the review
            await _reviews.DeleteOneAsync(r => r.Id == id);

            // Recalculate the vendor's average rating
            await UpdateVendorRating(review.VendorId);

            return Ok();
        }

        // Helper function to update vendor's average rating and review count
        private async Task UpdateVendorRating(string vendorId)
        {
            var vendor = await _vendors.Find(v => v.Id == vendorId).FirstOrDefaultAsync();
            if (vendor == null)
                return;

            // Calculate new average rating
            var allVendorReviews = await _reviews.Find(r => r.VendorId == vendorId).ToListAsync();
            if (allVendorReviews.Count > 0)
            {
                vendor.Rating = allVendorReviews.Average(r => r.Rating);
                vendor.ReviewCount = allVendorReviews.Count;
            }
            else
            {
                vendor.Rating = 0;
                vendor.ReviewCount = 0;
            }

            // Update vendor in the database
            await _vendors.ReplaceOneAsync(v => v.Id == vendorId, vendor);
        }
    }
}
