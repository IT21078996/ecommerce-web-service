using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using EcommerceWebAPI.Models;
using EcommerceWebAPI.Data;
using System.Threading.Tasks;

namespace EcommerceWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IMongoCollection<Notification> _notifications;

        public NotificationController(MongoDBContext dbContext)
        {
            _notifications = dbContext.GetCollection<Notification>("notifications");
        }

        // Get all notifications for a specific vendor
        [HttpGet("{vendorId}")]
        public async Task<IEnumerable<Notification>> GetNotificationsByVendorId(string vendorId)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.VendorId, vendorId);
            return await _notifications.Find(filter).ToListAsync();
        }

        // Mark a notification as read
        [HttpPut("markAsRead/{id}")]
        public async Task<ActionResult> MarkNotificationAsRead(string id)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, id);
            var update = Builders<Notification>.Update.Set(n => n.IsRead, true);

            var result = await _notifications.UpdateOneAsync(filter, update);
            if (result.ModifiedCount == 0)
            {
                return NotFound();
            }
            return Ok();
        }

        [HttpGet]
        public async Task<IEnumerable<Notification>> GetAllNotifications()
        {
            // Fetch all notifications without filtering by vendor
            return await _notifications.Find(FilterDefinition<Notification>.Empty).ToListAsync();
        }
    }
}