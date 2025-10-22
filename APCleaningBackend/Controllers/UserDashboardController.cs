using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APCleaningBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserDashboardController : ControllerBase
    {

        private readonly APCleaningBackendContext _context;

        public UserDashboardController(APCleaningBackendContext context)
        {
            _context = context;
        }

        [HttpGet("GetUpcomingBookings")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetUpcomingBookings(string userID)
        {
            var bookings = await _context.Booking
                .Include(b => b.ServiceType)
                .Select(b => new {
                    b.BookingID,
                    b.CustomerID,
                    b.FullName,
                    b.Email,
                    b.Address,
                    b.City,
                    b.Province,
                    b.ServiceDate,
                    b.ServiceStartTime,
                    b.ServiceEndTime,
                    b.BookingAmount,
                    b.BookingStatus,
                    b.PaymentStatus,
                    b.AssignedCleanerID,
                    b.AssignedDriverID,
                    ServiceTypeName = b.ServiceType.Name
                }).Where(b => b.ServiceDate > DateTime.Now && b.CustomerID == userID)
        .ToListAsync();

            return Ok(bookings);

        }


        [HttpGet("GetBookingHistory")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookingHistory(string userID)
        {
            var bookings = await _context.Booking
                .Include(b => b.ServiceType)
                .Select(b => new {
                    b.BookingID,
                    b.CustomerID,
                    b.FullName,
                    b.Email,
                    b.Address,
                    b.City,
                    b.Province,
                    b.ServiceDate,
                    b.ServiceStartTime,
                    b.ServiceEndTime,
                    b.BookingAmount,
                    b.BookingStatus,
                    b.PaymentStatus,
                    b.AssignedCleanerID,
                    b.AssignedDriverID,
                    ServiceTypeName = b.ServiceType.Name
                }).Where(b => b.ServiceDate < DateTime.Now && b.CustomerID == userID)
        .ToListAsync();

            return Ok(bookings);

        }

    }
}