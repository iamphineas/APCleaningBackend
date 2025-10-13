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

namespace APCleaningBackend.Controllers
{
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly APCleaningBackendContext _context;

        public AdminController(APCleaningBackendContext context)
        {
            _context = context;
        }

        // GET: Admin
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
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
                })
        .ToListAsync();

            return Ok(bookings);

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

        // POST: Admin/Edit/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] Booking updated)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking == null) return NotFound();

            booking.ServiceDate = updated.ServiceDate;
            booking.ServiceStartTime = updated.ServiceStartTime;
            booking.ServiceEndTime = updated.ServiceEndTime;
            booking.BookingStatus = updated.BookingStatus;
            booking.AssignedCleanerID = updated.AssignedCleanerID;
            booking.AssignedDriverID = updated.AssignedDriverID;
            booking.BookingAmount = updated.BookingAmount;

            await _context.SaveChangesAsync();
            return Ok(booking);
        }


        // POST: Admin/Delete/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking == null) return NotFound();

            _context.Booking.Remove(booking);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
