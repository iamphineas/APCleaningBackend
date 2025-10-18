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
using Microsoft.AspNetCore.Authorization;

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

        [HttpPut("bookings/{id}/reset-assignment")]
        public async Task<IActionResult> ResetBookingAssignment(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking == null) return NotFound("Booking not found.");

            var cleaner = await _context.CleanerDetails.FirstOrDefaultAsync(c => c.CleanerDetailsID == booking.AssignedCleanerID);
            var driver = await _context.DriverDetails.FirstOrDefaultAsync(d => d.DriverDetailsID == booking.AssignedDriverID);

            if (cleaner != null) cleaner.AvailabilityStatus = "Available";
            if (driver != null) driver.AvailabilityStatus = "Available";

            booking.AssignedCleanerID = null;
            booking.AssignedDriverID = null;
            if (booking.PaymentStatus == "Paid")
            {
                booking.BookingStatus = "Confirmed";
            }
            else
            {
                booking.BookingStatus = "Pending";
            }


            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Booking assignment reset. Cleaner and driver marked available.",
                bookingId = booking.BookingID
            });
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

            // ✅ Add these missing fields:
            booking.FullName = updated.FullName;
            booking.Email = updated.Email;
            booking.Address = updated.Address;
            booking.City = updated.City;
            booking.Province = updated.Province;
            booking.ZipCode = updated.ZipCode;
            booking.CustomerID = updated.CustomerID;
            booking.ServiceTypeID = updated.ServiceTypeID;

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

        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics()
        {
            var totalBookings = await _context.Booking.CountAsync();

            var totalRevenue = await _context.Booking
                .Where(b => b.PaymentStatus == "Paid")
                .SumAsync(b => (decimal?)b.BookingAmount) ?? 0;

            var completedCount = await _context.Booking
                .CountAsync(b => b.BookingStatus == "Completed");

            var completionRate = totalBookings == 0
                ? 0
                : Math.Round((double)completedCount / totalBookings * 100, 2);

            var unassignedCount = await _context.Booking
                .CountAsync(b => b.PaymentStatus == "Paid" && b.AssignedCleanerID == null);

            // Bookings by day
            var serviceDates = await _context.Booking
                .Select(b => b.ServiceDate)
                .ToListAsync();

            var bookingsByDay = serviceDates
                .GroupBy(date => date.DayOfWeek)
                .Select(g => new { Day = g.Key.ToString(), Count = g.Count() })
                .ToDictionary(x => x.Day, x => x.Count);

            // Revenue by month
            var paidBookings = await _context.Booking
                .Where(b => b.PaymentStatus == "Paid")
                .Select(b => new { b.ServiceDate, b.BookingAmount })
                .ToListAsync();

            var revenueByMonth = paidBookings
                .GroupBy(b => b.ServiceDate.ToString("MMM"))
                .Select(g => new { Month = g.Key, Total = g.Sum(b => b.BookingAmount) })
                .ToDictionary(x => x.Month, x => x.Total);

            // Cleaner performance
            var completedCleanerBookings = await _context.Booking
                .Where(b => b.AssignedCleanerID != null &&
                            b.BookingStatus == "Completed" &&
                            b.ServiceStartTime != null &&
                            b.ServiceEndTime != null)
                .ToListAsync();

            var cleanerPerformance = completedCleanerBookings
                .GroupBy(b => b.AssignedCleanerID)
                .Select(g =>
                {
                    var cleaner = _context.CleanerDetails.FirstOrDefault(c => c.CleanerDetailsID == g.Key);
                    var user = cleaner != null ? _context.Users.FirstOrDefault(u => u.Id == cleaner.UserId) : null;

                    return new
                    {
                        name = user?.FullName ?? "Unknown",
                        jobsCompleted = g.Count(),
                        avgJobDuration = Math.Round(g
                            .Select(b => (b.ServiceEndTime - b.ServiceStartTime).TotalMinutes / 60.0)
                            .Average(), 2)
                    };
                }).ToList();

            // Driver efficiency
            var completedDriverBookings = await _context.Booking
                .Where(b => b.AssignedDriverID != null &&
                            b.BookingStatus == "Completed" &&
                            b.ServiceStartTime != null &&
                            b.ServiceEndTime != null)
                .ToListAsync();

            var driverEfficiency = completedDriverBookings
                .GroupBy(b => b.AssignedDriverID)
                .Select(g =>
                {
                    var driver = _context.DriverDetails.FirstOrDefault(d => d.DriverDetailsID == g.Key);
                    var user = driver != null ? _context.Users.FirstOrDefault(u => u.Id == driver.UserId) : null;

                    return new
                    {
                        name = user?.FullName ?? "Unknown",
                        deliveriesCompleted = g.Count(),
                        avgDeliveryTime = Math.Round(g
                            .Select(b => (b.ServiceEndTime - b.ServiceStartTime).TotalMinutes / 60.0)
                            .Average(), 2)
                    };
                }).ToList();

            // Revenue by service type
            var revenueByServiceTypeRaw = await _context.Booking
                .Where(b => b.PaymentStatus == "Paid")
                .Select(b => new { b.ServiceTypeID, b.BookingAmount })
                .ToListAsync();

            var revenueByServiceType = revenueByServiceTypeRaw
                .GroupBy(b => b.ServiceTypeID)
                .Select(g =>
                {
                    var service = _context.ServiceType.FirstOrDefault(s => s.ServiceTypeID == g.Key);
                    return new
                    {
                        service = service?.Name ?? "Unknown",
                        revenue = g.Sum(b => b.BookingAmount)
                    };
                })
                .ToDictionary(x => x.service, x => x.revenue);

            // Service type breakdown
            var serviceTypeBreakdownRaw = await _context.Booking
                .Select(b => b.ServiceTypeID)
                .ToListAsync();

            var serviceTypeBreakdown = serviceTypeBreakdownRaw
                .GroupBy(id => id)
                .Select(g =>
                {
                    var service = _context.ServiceType.FirstOrDefault(s => s.ServiceTypeID == g.Key);
                    return new
                    {
                        service = service?.Name ?? "Unknown",
                        count = g.Count()
                    };
                })
                .ToDictionary(x => x.service, x => x.count);

            // Peak booking times
            var peakRaw = await _context.Booking
                .Where(b => b.ServiceStartTime != null)
                .Select(b => new { b.ServiceDate, b.ServiceStartTime })
                .ToListAsync();

            var peakBookingTimes = peakRaw
                .GroupBy(b => b.ServiceStartTime.ToString("HH:mm"))
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(b => b.ServiceDate.DayOfWeek.ToString())
                          .ToDictionary(x => x.Key, x => x.Count())
                );

            var response = new
            {
                totalBookings,
                totalRevenue,
                completionRate,
                unassignedCount,
                bookingsByDay,
                revenueByMonth,
                cleanerPerformance,
                driverEfficiency,
                revenueByServiceType,
                serviceTypeBreakdown,
                peakBookingTimes
            };

            return Ok(response);
        }
    }
}
