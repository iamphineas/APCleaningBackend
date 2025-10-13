using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Model;
using APCleaningBackend.ViewModel;

namespace APCleaningBackend.Controllers
{
    [ApiController]
    [Authorize(Roles = "Cleaner")]
    [Route("api/[controller]")]
    public class CleanerDashboardController : ControllerBase
    {
        private readonly APCleaningBackendContext _context;

        public CleanerDashboardController(APCleaningBackendContext context)
        {
            _context = context;
        }

        [HttpGet("bookings")]
        public async Task<IActionResult> GetAssignedBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cleaner = await _context.CleanerDetails
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cleaner == null) return NotFound("Cleaner profile not found.");

            var bookings = await _context.Booking
                .Where(b => b.AssignedCleanerID == cleaner.CleanerDetailsID)
                .Include(b => b.ServiceType) // Include related service
                .OrderByDescending(b => b.ServiceDate)
                .Select(b => new
                {
                    b.BookingID,
                    b.ServiceDate,
                    b.ServiceStartTime,
                    b.ServiceEndTime,
                    b.BookingStatus,
                    b.Address,
                    b.City,
                    b.Province,
                    ServiceName = b.ServiceType.Name // Send service name
                })
                .ToListAsync();

            return Ok(bookings);
        }

        // Get current availability status
        [HttpGet("availability")]
        public async Task<IActionResult> GetAvailability()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cleaner = await _context.CleanerDetails.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cleaner == null) return NotFound();

            return Ok(new { status = cleaner.AvailabilityStatus });
        }

        // Toggle availability
        [HttpPut("availability")]
        public async Task<IActionResult> SetAvailability([FromBody] bool isAvailable)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cleaner = await _context.CleanerDetails.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cleaner == null) return NotFound();

            cleaner.AvailabilityStatus = isAvailable ? "Available" : "Unavailable";
            await _context.SaveChangesAsync();

            return Ok(new { status = cleaner.AvailabilityStatus });
        }

        [HttpPut("bookings/{id}/status")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] string newStatus)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cleaner = await _context.CleanerDetails.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cleaner == null) return NotFound();

            var booking = await _context.Booking.FindAsync(id);
            if (booking == null || booking.AssignedCleanerID != cleaner.CleanerDetailsID)
                return Unauthorized();

            booking.BookingStatus = newStatus;

            if (newStatus == "Completed")
            {
                cleaner.AvailabilityStatus = "Available";

                var driver = await _context.DriverDetails
                    .FirstOrDefaultAsync(d => d.DriverDetailsID == booking.AssignedDriverID);

                if (driver != null)
                {
                    driver.AvailabilityStatus = "Available";
                }
            }

            await _context.SaveChangesAsync();
            return Ok(booking);
        }

        // Optional: Get notifications
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = await _context.Notification
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }

        // Optional: Mark notification as read
        [HttpPut("notifications/{id}/read")]
        public async Task<IActionResult> MarkNotificationRead(int id)
        {
            var notification = await _context.Notification.FindAsync(id);
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}