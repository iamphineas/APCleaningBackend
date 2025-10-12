using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using APCleaningBackend.Model;
using APCleaningBackend.Areas.Identity.Data;

namespace APCleaningBackend.Controllers
{
    [ApiController]
    [Authorize(Roles = "Driver")]
    [Route("api/[controller]")]
    public class DriverDashboardController : ControllerBase
    {
        private readonly APCleaningBackendContext _context;

        public DriverDashboardController(APCleaningBackendContext context)
        {
            _context = context;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        // Get bookings assigned to the current driver
        [HttpGet("bookings")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetAssignedBookings()
        {
            var userId = GetUserId();
            var driver = await _context.DriverDetails.FirstOrDefaultAsync(d => d.UserId == userId);
            if (driver == null) return Unauthorized();

            var bookings = await _context.Booking
                .Where(b => b.AssignedDriverID == driver.DriverDetailsID)
                .OrderByDescending(b => b.ServiceDate)
                .ToListAsync();

            return Ok(bookings);
        }

        // Get driver's availability
        [HttpGet("availability")]
        public async Task<IActionResult> GetAvailability()
        {
            var userId = GetUserId();
            var driver = await _context.DriverDetails.FirstOrDefaultAsync(d => d.UserId == userId);
            if (driver == null) return Unauthorized();

            return Ok(new { status = driver.AvailabilityStatus });
        }

        // Toggle driver's availability
        [HttpPut("availability")]
        public async Task<IActionResult> UpdateAvailability([FromBody] bool isAvailable)
        {
            var userId = GetUserId();
            var driver = await _context.DriverDetails.FirstOrDefaultAsync(d => d.UserId == userId);
            if (driver == null) return Unauthorized();

            driver.AvailabilityStatus = isAvailable ? "Available" : "Unavailable";
            await _context.SaveChangesAsync();

            return Ok(new { status = driver.AvailabilityStatus });
        }

        [HttpPut("availability")]
        public async Task<IActionResult> MarkDriverAvailable()
        {
            var userId = GetUserId();
            var driver = await _context.DriverDetails.FirstOrDefaultAsync(d => d.UserId == userId);
            if (driver == null) return Unauthorized();

            driver.AvailabilityStatus = "Available";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Driver marked available" });
        }

        // Optional: driver notifications
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = GetUserId();
            var notifications = await _context.Notification
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpPut("notifications/{id}/read")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            var notification = await _context.Notification.FindAsync(id);
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}