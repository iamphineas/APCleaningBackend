using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Controllers;
using APCleaningBackend.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace APCleaningBackendTest
{
    public class DriverDashboardControllerTests
    {
        private const string TestUserId = "driver-123";

        private DriverDashboardController CreateControllerWithContext(string dbName, out APCleaningBackendContext context)
        {
            var options = new DbContextOptionsBuilder<APCleaningBackendContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .Options;

            context = new APCleaningBackendContext(options);

            var controller = new DriverDashboardController(context);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, TestUserId),
                new Claim(ClaimTypes.Role, "Driver")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        private static DriverDetails CreateDriver() => new DriverDetails
        {
            DriverDetailsID = 1,
            UserId = TestUserId,
            AvailabilityStatus = "Unavailable",
            LicenseNumber = "DR123456",
            VehicleType = "Van"
        };

        private static Booking CreateBooking(int driverId) => new Booking
        {
            BookingID = 1,
            AssignedDriverID = driverId,
            ServiceTypeID = 1,
            ServiceDate = DateTime.Today,
            ServiceStartTime = DateTime.Today.AddHours(1),
            ServiceEndTime = DateTime.Today.AddHours(2),
            BookingStatus = "Scheduled",
            Address = "456 Main Rd",
            City = "Durban",
            Province = "KZN",
            ZipCode = "4001",
            FullName = "Jane Doe",
            Email = "jane@example.com",
            CustomerID = "cust-002"
        };

        [Fact]
        public async Task GetAvailability_ReturnsStatus()
        {
            var controller = CreateControllerWithContext("DriverAvailabilityDb", out var context);
            context.DriverDetails.Add(CreateDriver());
            await context.SaveChangesAsync();

            var result = await controller.GetAvailability();
            var ok = Assert.IsType<OkObjectResult>(result);
            var statusProp = ok.Value.GetType().GetProperty("status");
            var statusValue = statusProp?.GetValue(ok.Value)?.ToString();
            Assert.Equal("Unavailable", statusValue);
        }

        [Fact]
        public async Task UpdateAvailability_UpdatesStatus()
        {
            var controller = CreateControllerWithContext("DriverUpdateAvailabilityDb", out var context);
            context.DriverDetails.Add(CreateDriver());
            await context.SaveChangesAsync();

            var result = await controller.UpdateAvailability(true);
            var ok = Assert.IsType<OkObjectResult>(result);
            var statusProp = ok.Value.GetType().GetProperty("status");
            var statusValue = statusProp?.GetValue(ok.Value)?.ToString();
            Assert.Equal("Available", statusValue);
        }

        [Fact]
        public async Task GetAssignedBookings_ReturnsBookings()
        {
            var controller = CreateControllerWithContext("DriverBookingsDb", out var context);

            var driver = CreateDriver();
            var service = new ServiceType
            {
                ServiceTypeID = 1,
                Name = "Transport",
                Description = "Pickup and delivery",
                ImageURL = "transport.jpg"
            };

            context.DriverDetails.Add(driver);
            context.ServiceType.Add(service);
            context.Booking.Add(CreateBooking(driver.DriverDetailsID));

            await context.SaveChangesAsync();

            var result = await controller.GetAssignedBookings();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var bookings = Assert.IsAssignableFrom<IEnumerable<Booking>>(ok.Value);
            Assert.Single(bookings);
        }

        [Fact]
        public async Task MarkDriverAvailable_SetsStatus()
        {
            var controller = CreateControllerWithContext("DriverMarkAvailableDb", out var context);
            context.DriverDetails.Add(CreateDriver());
            await context.SaveChangesAsync();

            var result = await controller.MarkDriverAvailable();
            var ok = Assert.IsType<OkObjectResult>(result);
            var messageProp = ok.Value.GetType().GetProperty("message");
            var messageValue = messageProp?.GetValue(ok.Value)?.ToString();
            Assert.Equal("Driver marked available", messageValue);
        }

        [Fact]
        public async Task GetNotifications_ReturnsList()
        {
            var controller = CreateControllerWithContext("DriverNotificationsDb", out var context);

            context.Notification.Add(new Notification
            {
                Id = 1,
                UserId = TestUserId,
                Message = "Driver alert",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });

            await context.SaveChangesAsync();

            var result = await controller.GetNotifications();
            var ok = Assert.IsType<OkObjectResult>(result);
            var notifications = Assert.IsAssignableFrom<List<Notification>>(ok.Value);
            Assert.Single(notifications);
        }

        [Fact]
        public async Task MarkNotificationAsRead_ValidId_SetsIsReadTrue()
        {
            var controller = CreateControllerWithContext("DriverMarkReadDb", out var context);

            context.Notification.Add(new Notification
            {
                Id = 1,
                UserId = TestUserId,
                Message = "Unread",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });

            await context.SaveChangesAsync();

            var result = await controller.MarkNotificationAsRead(1);
            Assert.IsType<OkResult>(result);

            var updated = await context.Notification.FindAsync(1);
            Assert.True(updated.IsRead);
        }
    }
}