using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Controllers;
using APCleaningBackend.Model;
using APCleaningBackend.Services;
using APCleaningBackend.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace APCleaningBackendTest
{
    public class BookingControllerTests
    {
        private readonly Mock<IConfiguration> _mockConfig;

        public BookingControllerTests()
        {
            _mockConfig = new Mock<IConfiguration>();
        }

        private (BookingsController controller, APCleaningBackendContext context) CreateControllerWithContext(string dbName = null, IEmailService emailService = null)
        {
            var options = new DbContextOptionsBuilder<APCleaningBackendContext>()
                .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            var context = new APCleaningBackendContext(options);
            var controller = new BookingsController(context, _mockConfig.Object, emailService ?? new Mock<IEmailService>().Object);

            return (controller, context);
        }

        [Fact]
        public async Task GetBookings_ReturnsOkResult()
        {
            var (controller, context) = CreateControllerWithContext();

            context.ServiceType.Add(new ServiceType
            {
                Name = "Deep Clean",
                Description = "Thorough cleaning of all rooms and surfaces",
                ImageURL = "https://example.com/image.jpg",
                Price = 500
            });

            await context.SaveChangesAsync();

            var serviceType = await context.ServiceType.FirstAsync();

            context.Booking.Add(new Booking
            {
                FullName = "Alwande",
                Email = "alwandengcobo3@gmail.com",
                Address = "123 Main St",
                City = "Durban",
                Province = "KwaZulu-Natal",
                ZipCode = "4001",
                CustomerID = "test-user-id",
                ServiceTypeID = serviceType.ServiceTypeID
            });

            await context.SaveChangesAsync();

            var result = await controller.GetBookings();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetBooking_ValidId_ReturnsBooking()
        {
            var (controller, context) = CreateControllerWithContext();

            context.Booking.Add(new Booking
            {
                FullName = "Alwande",
                Email = "alwandengcobo3@gmail.com",
                Address = "123 Main St",
                City = "Durban",
                Province = "KwaZulu-Natal",
                ZipCode = "4001",
                CustomerID = "test-user-id"
            });

            await context.SaveChangesAsync();
            var booking = await context.Booking.FirstAsync();

            var result = await controller.GetBooking(booking.BookingID);
            Assert.Equal(booking.BookingID, result.Value.BookingID);
        }

        [Fact]
        public async Task GetBooking_InvalidId_ReturnsNotFound()
        {
            var (controller, _) = CreateControllerWithContext();
            var result = await controller.GetBooking(999);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostBooking_ValidModel_ReturnsRedirectUrl()
        {
            var (controller, context) = CreateControllerWithContext();

            _mockConfig.Setup(c => c["Payfast:MerchantId"]).Returns("10000100");
            _mockConfig.Setup(c => c["Payfast:MerchantKey"]).Returns("abc123");
            _mockConfig.Setup(c => c["Payfast:UseSandbox"]).Returns("true");
            _mockConfig.Setup(c => c["Payfast:SandboxUrl"]).Returns("https://sandbox.payfast.co.za/eng/process");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "test-user-id")
                    }, "mock"))
                }
            };

            context.ServiceType.Add(new ServiceType
            {
                Name = "Deep Clean",
                Description = "Thorough cleaning",
                ImageURL = "https://example.com/image.jpg",
                Price = 500
            });

            await context.SaveChangesAsync();
            var serviceType = await context.ServiceType.FirstAsync();

            var model = new BookingViewModel
            {
                FullName = "Alwande",
                Email = "alwandengcobo3@gmail.com",
                Address = "123 Main St",
                City = "Durban",
                Province = "KwaZulu-Natal",
                ZipCode = "4001",
                CustomerID = null,
                BookingAmount = 500,
                ServiceTypeID = serviceType.ServiceTypeID,
                ServiceDate = DateTime.Today,
                ServiceStartTime = DateTime.Now,
                ServiceEndTime = DateTime.Now.AddHours(2),
                CreatedDate = DateTime.Now
            };

            var result = await controller.PostBooking(model);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Contains("redirectUrl", ok.Value.ToString());
        }

        [Fact]
        public async Task HandleNotification_CompleteStatus_UpdatesBookingToPaid()
        {
            var mockEmailService = new Mock<IEmailService>();
            mockEmailService.Setup(x => x.SendInvoiceAsync(It.IsAny<Booking>()))
                            .Returns(Task.CompletedTask);

            var (controller, context) = CreateControllerWithContext(emailService: mockEmailService.Object);

            context.ServiceType.Add(new ServiceType
            {
                Name = "Deep Clean",
                Description = "Thorough cleaning",
                ImageURL = "https://example.com/image.jpg",
                Price = 500
            });

            await context.SaveChangesAsync();
            var serviceType = await context.ServiceType.FirstAsync();

            context.Booking.Add(new Booking
            {
                FullName = "Alwande",
                Email = "alwandengcobo3@gmail.com",
                Address = "123 Main St",
                City = "Durban",
                Province = "KwaZulu-Natal",
                ZipCode = "4001",
                CustomerID = "test-user-id",
                BookingAmount = 500,
                ServiceStartTime = DateTime.Now,
                ServiceEndTime = DateTime.Now.AddHours(2),
                ServiceTypeID = serviceType.ServiceTypeID
            });

            await context.SaveChangesAsync();
            var booking = await context.Booking.FirstAsync();

            var form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "payment_status", "COMPLETE" },
                { "item_name", $"Booking #{booking.BookingID}" }
            });

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Form = form;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var result = await controller.HandleNotification();
            Assert.IsType<OkResult>(result);

            var updated = await context.Booking.FindAsync(booking.BookingID);
            Assert.Equal("Paid", updated.PaymentStatus);

            mockEmailService.Verify(x => x.SendInvoiceAsync(It.IsAny<Booking>()), Times.Once);
        }

        [Fact]
        public async Task UpdateBookingStatus_Completed_UpdatesAvailability()
        {
            var (controller, context) = CreateControllerWithContext();

            var cleaner = new CleanerDetails
            {
                UserId = "cleaner-user-id",
                AvailabilityStatus = "Busy"
            };

            var driver = new DriverDetails
            {
                UserId = "cleaner-user-id",
                AvailabilityStatus = "Busy",
                LicenseNumber = "DR123456",
                VehicleType = "Van"
            };

            context.CleanerDetails.Add(cleaner);
            context.DriverDetails.Add(driver);
            await context.SaveChangesAsync();

            var service = new ServiceType
            {
                Name = "Basic Clean",
                Description = "Quick clean",
                ImageURL = "basic.jpg",
                Price = 150
            };

            context.ServiceType.Add(service);
            await context.SaveChangesAsync();

            var booking = new Booking
            {
                BookingStatus = "Confirmed",
                AssignedCleanerID = cleaner.CleanerDetailsID,
                AssignedDriverID = driver.DriverDetailsID,
                FullName = "Alwande",
                Email = "alwandengcobo3@gmail.com",
                Address = "123 Main St",
                City = "Durban",
                Province = "KwaZulu-Natal",
                ZipCode = "4001",
                CustomerID = "test-user-id",
                ServiceTypeID = service.ServiceTypeID
            };

            context.Booking.Add(booking);
            await context.SaveChangesAsync();

            var result = await controller.UpdateBookingStatus(booking.BookingID, "Completed");
            var ok = Assert.IsType<OkObjectResult>(result);
            var updatedBooking = Assert.IsType<Booking>(ok.Value);
            Assert.Equal("Completed", updatedBooking.BookingStatus);

            var updatedCleaner = await context.CleanerDetails.FindAsync(cleaner.CleanerDetailsID);
            var updatedDriver = await context.DriverDetails.FindAsync(driver.DriverDetailsID);
            Assert.Equal("Available", updatedCleaner.AvailabilityStatus);
            Assert.Equal("Available", updatedDriver.AvailabilityStatus);
        }
    }
}