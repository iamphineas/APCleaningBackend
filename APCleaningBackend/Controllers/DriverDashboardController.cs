using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using APCleaningBackend.Model;
using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Services;

namespace APCleaningBackend.Controllers
{
    [ApiController]
    [Authorize(Roles = "Driver")]
    [Route("api/[controller]")]
    public class DriverDashboardController : ControllerBase
    {
        private readonly APCleaningBackendContext _context;
        private readonly IEmailService _emailService;

        public DriverDashboardController(APCleaningBackendContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        // Get bookings assigned to the current driver
        [HttpGet("bookings")]
        public async Task<ActionResult<IEnumerable<object>>> GetAssignedBookings()
        {
            var userId = GetUserId();
            var driver = await _context.DriverDetails.FirstOrDefaultAsync(d => d.UserId == userId);
            if (driver == null) return Unauthorized();

            var bookings = await (from b in _context.Booking
                                  join st in _context.ServiceType
                                  on b.ServiceTypeID equals st.ServiceTypeID

                                  join cd in _context.CleanerDetails
                                  on b.AssignedCleanerID equals cd.CleanerDetailsID into cleanerJoin
                                  from cd in cleanerJoin.DefaultIfEmpty()

                                  join cu in _context.Users
                                  on cd.UserId equals cu.Id into cleanerUserJoin
                                  from cu in cleanerUserJoin.DefaultIfEmpty()

                                  join dd in _context.DriverDetails
                                  on b.AssignedDriverID equals dd.DriverDetailsID into driverJoin
                                  from dd in driverJoin.DefaultIfEmpty()

                                  join du in _context.Users
                                  on dd.UserId equals du.Id into driverUserJoin
                                  from du in driverUserJoin.DefaultIfEmpty()

                                  where b.AssignedDriverID == driver.DriverDetailsID
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
                                      CleanerName = cu.FullName,
                                      CleanerImageUrl = cd.CleanerImageUrl,
                                      DriverName = du.FullName,
                                      DriverImageUrl = dd.DriverImageUrl,
                                      AssignedDriverID = b.AssignedDriverID
                                  }).ToListAsync();

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

        // Optional: mark driver available directly
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

        // Driver notifications
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

        // Log dispatch note
        [HttpPost("DispatchNote")]
        public async Task<IActionResult> LogDispatchNote([FromBody] DispatchNote note)
        {
            if (string.IsNullOrWhiteSpace(note.Note))
                return BadRequest("Note cannot be empty.");

            try
            {
                _context.DispatchNotes.Add(note);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Note logged successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error logging dispatch note: " + ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("bookings/{id}/driver-status")]
        public async Task<IActionResult> UpdateDriverBookingStatus(int id, [FromBody] string newStatus)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var driver = await _context.DriverDetails.FirstOrDefaultAsync(d => d.UserId == userId);
            if (driver == null) return NotFound();

            var booking = await _context.Booking
    .Include(b => b.ServiceType)
    .Include(b => b.AssignedDriver)
        .ThenInclude(d => d.User)
    .Include(b => b.AssignedCleaner)
        .ThenInclude(c => c.User)
    .FirstOrDefaultAsync(b => b.BookingID == id);
            if (booking == null || booking.AssignedDriverID != driver.DriverDetailsID)
                return Unauthorized();

            if (newStatus != "EnRoute" && newStatus != "Arrived")
                return BadRequest("Invalid status. Drivers can only mark EnRoute or Arrived.");

            booking.BookingStatus = newStatus;
            await _context.SaveChangesAsync();

            await _emailService.SendDriverStatusToCustomerAsync(booking);

            return Ok(new { message = $"Booking marked as {newStatus}." });
        }
    }
}