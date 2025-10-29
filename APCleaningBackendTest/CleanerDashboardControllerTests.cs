using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Controllers;
using APCleaningBackend.Model;
using APCleaningBackend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace APCleaningBackendTest
{
    public class CleanerDashboardControllerTests
    {
        private const string TestUserId = "cleaner-123";

        private CleanerDashboardController CreateControllerWithContext(string dbName, out APCleaningBackendContext context, out Mock<IEmailService> mockEmailService)
        {
            var options = new DbContextOptionsBuilder<APCleaningBackendContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .Options;

            context = new APCleaningBackendContext(options);

            // Mock the email service
            mockEmailService = new Mock<IEmailService>();
            mockEmailService
                .Setup(service => service.SendServiceCompleteToCustomerAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            var controller = new CleanerDashboardController(context, mockEmailService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, TestUserId),
                new Claim(ClaimTypes.Role, "Cleaner")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        private static CleanerDetails CreateCleaner() => new CleanerDetails
        {
            UserId = TestUserId,
            AvailabilityStatus = "Unavailable",
            ServiceTypeID = 1,
            CleanerImageUrl = "test-cleaner.jpg"
        };

        private static Booking CreateBooking(int cleanerId, int serviceTypeId, int driverId = 0) => new Booking
        {
            AssignedCleanerID = cleanerId,
            AssignedDriverID = driverId,
            ServiceTypeID = serviceTypeId,
            ServiceDate = DateTime.Today,
            ServiceStartTime = DateTime.Today.AddHours(1),
            ServiceEndTime = DateTime.Today.AddHours(2),
            BookingStatus = "Scheduled",
            Address = "123 Main St",
            City = "Durban",
            Province = "KZN",
            ZipCode = "4001",
            FullName = "John Doe",
            Email = "john@example.com",
            CustomerID = "cust-001",
            BookingAmount = 150
        };

        [Fact]
        public async Task GetAvailability_ReturnsStatus()
        {
            var controller = CreateControllerWithContext(Guid.NewGuid().ToString(), out var context, out _);
            context.CleanerDetails.Add(CreateCleaner());
            await context.SaveChangesAsync();

            var result = await controller.GetAvailability();
            var ok = Assert.IsType<OkObjectResult>(result);
            var statusProp = ok.Value.GetType().GetProperty("status");
            var statusValue = statusProp?.GetValue(ok.Value)?.ToString();
            Assert.Equal("Unavailable", statusValue);
        }

        [Fact]
        public async Task SetAvailability_UpdatesStatus()
        {
            var controller = CreateControllerWithContext(Guid.NewGuid().ToString(), out var context, out _);
            context.CleanerDetails.Add(CreateCleaner());
            await context.SaveChangesAsync();

            var result = await controller.SetAvailability(true);
            var ok = Assert.IsType<OkObjectResult>(result);
            var statusProp = ok.Value.GetType().GetProperty("status");
            var statusValue = statusProp?.GetValue(ok.Value)?.ToString();
            Assert.Equal("Available", statusValue);
        }

        [Fact]
        public async Task GetAssignedBookings_ReturnsBookings()
        {
            var controller = CreateControllerWithContext(Guid.NewGuid().ToString(), out var context, out _);

            var cleaner = CreateCleaner();
            var service = new ServiceType
            {
                Name = "Deep Clean",
                Description = "Thorough cleaning service",
                ImageURL = "service.jpg",
                Price = 250
            };

            context.CleanerDetails.Add(cleaner);
            context.ServiceType.Add(service);
            await context.SaveChangesAsync();

            var booking = CreateBooking(cleaner.CleanerDetailsID, service.ServiceTypeID);
            context.Booking.Add(booking);
            await context.SaveChangesAsync();

            var result = await controller.GetAssignedBookings();
            var ok = Assert.IsType<OkObjectResult>(result);
            var bookings = ok.Value as IEnumerable<object>;
            Assert.NotNull(bookings);
        }

        [Fact]
        public async Task UpdateBookingStatus_Valid_UpdatesStatusAndAvailability_AndSendsEmail()
        {
            var controller = CreateControllerWithContext(Guid.NewGuid().ToString(), out var context, out var mockEmailService);

            var cleaner = CreateCleaner();
            var driver = new DriverDetails
            {
                AvailabilityStatus = "Unavailable",
                LicenseNumber = "DR123456",
                UserId = "driver-456",
                VehicleType = "Van",
                DriverImageUrl = "test-driver.jpg"
            };

            var service = new ServiceType
            {
                Name = "Basic Clean",
                Description = "Quick clean",
                ImageURL = "basic.jpg",
                Price = 150
            };

            context.CleanerDetails.Add(cleaner);
            context.DriverDetails.Add(driver);
            context.ServiceType.Add(service);
            await context.SaveChangesAsync();

            var booking = CreateBooking(cleaner.CleanerDetailsID, service.ServiceTypeID, driver.DriverDetailsID);
            context.Booking.Add(booking);
            await context.SaveChangesAsync();

            var result = await controller.UpdateBookingStatus(booking.BookingID, "Completed");
            var ok = Assert.IsType<OkObjectResult>(result);
            var updated = Assert.IsType<Booking>(ok.Value);

            Assert.Equal("Completed", updated.BookingStatus);
            Assert.Equal("Available", cleaner.AvailabilityStatus);
            Assert.Equal("Available", driver.AvailabilityStatus);

            // Verify that the email service was triggered
            mockEmailService.Verify(s => s.SendServiceCompleteToCustomerAsync(It.IsAny<Booking>()), Times.Once);
        }

        [Fact]
        public async Task GetNotifications_ReturnsList()
        {
            var controller = CreateControllerWithContext(Guid.NewGuid().ToString(), out var context, out _);

            context.Notification.Add(new Notification
            {
                UserId = TestUserId,
                Message = "Test notification",
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
        public async Task MarkNotificationRead_ValidId_SetsIsReadTrue()
        {
            var controller = CreateControllerWithContext(Guid.NewGuid().ToString(), out var context, out _);

            context.Notification.Add(new Notification
            {
                UserId = TestUserId,
                Message = "Unread",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });

            await context.SaveChangesAsync();

            var notification = await context.Notification.FirstAsync();
            var result = await controller.MarkNotificationRead(notification.Id);
            Assert.IsType<OkResult>(result);

            var updated = await context.Notification.FindAsync(notification.Id);
            Assert.True(updated.IsRead);
        }
    }
}
