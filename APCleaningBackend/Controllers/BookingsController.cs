using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Model;
using APCleaningBackend.ViewModel;
using Microsoft.AspNetCore.Identity;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace APCleaningBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly APCleaningBackendContext _context;
        private readonly IConfiguration _config;


        public BookingsController(APCleaningBackendContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: Booking
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            return await _context.Booking.ToListAsync();
        }

        // GET: api/DriverDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var driver = await _context.Booking.FindAsync(id);
            if (driver == null)
            {
                return NotFound();
            }

            return driver;
        }

        // POST: api/CleanerDetails
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<Booking>> PostBooking([FromBody] BookingViewModel model)
        {
            Console.WriteLine("🔍 Incoming booking request");
            Console.WriteLine($"🔐 IsAuthenticated: {User.Identity.IsAuthenticated}");
            Console.WriteLine($"🧾 Claims count: {User.Claims.Count()}");

            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"🔑 Claim Type: {claim.Type}, Value: {claim.Value}");
            }

            var userId = User.Identity.IsAuthenticated
    ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
    : model.CustomerID ?? $"guest-{DateTime.UtcNow.Ticks}";

            Console.WriteLine($"📌 Final CustomerID: {userId}");



            if (string.IsNullOrWhiteSpace(userId))
            {
                userId = $"guest-{DateTime.UtcNow.Ticks}";
            }


            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(errors);
            }

            var booking = new Booking
            {
                CustomerID = userId,
                AssignedDriverID = model.AssignedDriverID,
                AssignedCleanerID = model.AssignedCleanerID,
                ServiceTypeID = model.ServiceTypeID,
                ServiceDate = model.ServiceDate,
                ServiceStartTime = model.ServiceStartTime,
                ServiceEndTime = model.ServiceEndTime,
                BookingAmount = model.BookingAmount,
                CreatedDate = model.CreatedDate
            };



            try
            {

                _context.Booking.Add(booking);
                await _context.SaveChangesAsync();

                var merchantId = _config["Payfast:MerchantId"];
                var merchantKey = _config["Payfast:MerchantKey"];

                Console.WriteLine("Formatted amount for PayFast: " + booking.BookingAmount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));

                var data = new Dictionary<string, string>
                {
                    { "merchant_id", merchantId },
                    { "merchant_key", merchantKey },
                    { "return_url", "http://localhost:5173/payment-success" },
                    { "cancel_url", "http://localhost:5173/payment-cancelled" },
                    { "notify_url", "https://interverbal-plucky-joella.ngrok-free.dev/api/Bookings/payfast/notify" },
                    { "amount", booking.BookingAmount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) },
                    { "item_name", $"Booking #{booking.BookingID}" },
                    { "name_first", model.FullName },
                    { "email_address", model.Email }

                };

                var query = string.Join("&", data.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                var useSandbox = bool.Parse(_config["Payfast:UseSandbox"]);
                var baseUrl = useSandbox
                    ? _config["Payfast:SandboxUrl"]
                    : _config["Payfast:LiveUrl"];

                var redirectUrl = $"{baseUrl}?{query}";

                Console.WriteLine("🔗 Final PayFast Redirect URL:");
                Console.WriteLine(redirectUrl);


                return Ok(new { bookingId = booking.BookingID, redirectUrl });
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("payfast/notify")]
        public async Task<IActionResult> HandleNotification()
        {
            var form = await Request.ReadFormAsync();
            var paymentStatus = form["payment_status"];
            var bookingId = form["item_name"].ToString().Split('#')[1];

            var booking = await _context.Booking.FindAsync(int.Parse(bookingId));

            if (booking != null)
            {
                if (paymentStatus == "COMPLETE")
                {
                    booking.PaymentStatus = "Paid";
                    Console.WriteLine($"Booking #{booking.BookingID} marked as Paid.");
                }
                else
                {
                    booking.PaymentStatus = "Failed";
                    Console.WriteLine($"Booking #{booking.BookingID} marked as Failed (status: {paymentStatus}).");
                }

                await _context.SaveChangesAsync();
            }



            return Ok();
        }

        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignBooking(int id, [FromBody] BookingAssignmentModel model)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
                return NotFound();

            // Assign cleaner
            if (model.CleanerID.HasValue)
            {
                booking.AssignedCleanerID = model.CleanerID;

                var cleaner = await _context.CleanerDetails.FindAsync(model.CleanerID.Value);
                if (cleaner != null)
                {
                    cleaner.AvailabilityStatus = "Unavailable";

                    // Notify cleaner
                    _context.Notification.Add(new Notification
                    {
                        UserId = cleaner.UserId,
                        Message = $"New booking assigned for {booking.ServiceDate:MMM dd}",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    });
                }
            }

            // Assign driver
            if (model.DriverID.HasValue)
            {
                booking.AssignedDriverID = model.DriverID;

                var driver = await _context.DriverDetails.FindAsync(model.DriverID.Value);
                if (driver != null)
                {
                    driver.AvailabilityStatus = "Unavailable";

                    // Notify driver
                    _context.Notification.Add(new Notification
                    {
                        UserId = driver.UserId,
                        Message = $"New pickup assigned for {booking.ServiceDate:MMM dd}",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    });
                }
            }

            // Set booking status
            if (booking.AssignedCleanerID.HasValue && booking.AssignedDriverID.HasValue)
            {
                booking.BookingStatus = "Pending";
            }

            await _context.SaveChangesAsync();
            return Ok(booking);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] string newStatus)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
                return NotFound();

            booking.BookingStatus = newStatus;

            if (newStatus == "Completed")
            {
                if (booking.AssignedCleanerID.HasValue)
                {
                    var cleaner = await _context.CleanerDetails.FindAsync(booking.AssignedCleanerID.Value);
                    if (cleaner != null)
                        cleaner.AvailabilityStatus = "Available";
                }

                if (booking.AssignedDriverID.HasValue)
                {
                    var driver = await _context.DriverDetails.FindAsync(booking.AssignedDriverID.Value);
                    if (driver != null)
                        driver.AvailabilityStatus = "Available";
                }
            }

            await _context.SaveChangesAsync();
            return Ok(booking);
        }

        [HttpGet("cleaners")]
        public async Task<ActionResult<IEnumerable<CleanerViewModel>>> GetCleaners()
        {
            var cleaners = await (from cd in _context.CleanerDetails
                                  join user in _context.Users
                                  on cd.UserId equals user.Id
                                  join service in _context.ServiceType
                                  on cd.ServiceTypeID equals service.ServiceTypeID
                                  select new CleanerViewModel
                                  {
                                      CleanerDetailsID = cd.CleanerDetailsID,
                                      FullName = user.FullName,
                                      Email = user.Email,
                                      PhoneNumber = user.PhoneNumber,
                                      ServiceTypeID = cd.ServiceTypeID,
                                      ServiceName = service.Name,
                                      AvailabilityStatus = cd.AvailabilityStatus
                                  }).ToListAsync();

            return Ok(cleaners);
        }

        [HttpGet("drivers")]
        public async Task<IActionResult> GetDrivers()
        {
            var drivers = await (from dd in _context.DriverDetails
                                 join user in _context.Users
                                 on dd.UserId equals user.Id
                                 select new DriverViewModel
                                 {
                                     DriverDetailsID = dd.DriverDetailsID,
                                     FullName = user.FullName,
                                     Email = user.Email,
                                     PhoneNumber = user.PhoneNumber,
                                     LicenseNumber = dd.LicenseNumber,
                                     VehicleType = dd.VehicleType,
                                     AvailabilityStatus = dd.AvailabilityStatus
                                 }).ToListAsync();

            return Ok(drivers);

        }

    }
}
