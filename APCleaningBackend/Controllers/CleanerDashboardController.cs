using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Model;
using APCleaningBackend.ViewModel;
using APCleaningBackend.Services;

namespace APCleaningBackend.Controllers
{
    [ApiController]
    [Authorize(Roles = "Cleaner")]
    [Route("api/[controller]")]
    public class CleanerDashboardController : ControllerBase
    {
        private readonly APCleaningBackendContext _context;
        private readonly IEmailService _emailService;

        public CleanerDashboardController(APCleaningBackendContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet("bookings")]
        public async Task<IActionResult> GetAssignedBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cleaner = await _context.CleanerDetails
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cleaner == null) return NotFound("Cleaner profile not found.");

            var bookings = await (from b in _context.Booking
                                  join st in _context.ServiceType
                                  on b.ServiceTypeID equals st.ServiceTypeID

                                  join dd in _context.DriverDetails
                                  on b.AssignedDriverID equals dd.DriverDetailsID into driverJoin
                                  from dd in driverJoin.DefaultIfEmpty()

                                  join du in _context.Users
                                  on dd.UserId equals du.Id into driverUserJoin
                                  from du in driverUserJoin.DefaultIfEmpty()

                                  where b.AssignedCleanerID == cleaner.CleanerDetailsID
                                  orderby b.ServiceDate descending

                                  select new
                                  {
                                      b.BookingID,
                                      b.ServiceDate,
                                      b.ServiceStartTime,
                                      b.ServiceEndTime,
                                      b.BookingStatus,
                                      b.Address,
                                      b.City,
                                      b.Province,
                                      ServiceName = st.Name,
                                      DriverName = du.FullName
                                  }).ToListAsync();

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

            var booking = await _context.Booking.Include(b => b.ServiceType).FirstOrDefaultAsync(b => b.BookingID == id);
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

                await _emailService.SendServiceCompleteToCustomerAsync(booking);
            }

            await _context.SaveChangesAsync();
            return Ok(booking);
        }

        // Get notifications
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

        // Mark notification as read
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