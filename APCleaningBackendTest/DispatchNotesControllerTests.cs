using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Controllers;
using APCleaningBackend.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace APCleaningBackendTest
{
    public class DispatchNotesControllerTests
    {
        private DispatchNotesController CreateControllerWithContext(string dbName, out APCleaningBackendContext context)
        {
            var options = new DbContextOptionsBuilder<APCleaningBackendContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .Options;

            context = new APCleaningBackendContext(options);
            return new DispatchNotesController(context);
        }

        [Fact]
        public async Task GetAllDispatchNotes_ReturnsJoinedNotes()
        {
            var controller = CreateControllerWithContext("DispatchNotesDb", out var context);

            // Seed user
            var driverUser = new ApplicationUser
            {
                Id = "driver-001",
                FullName = "Driver One"
            };

            // Seed driver
            var driver = new DriverDetails
            {
                DriverDetailsID = 1,
                UserId = driverUser.Id,
                DriverImageUrl = "driver1.jpg",
                LicenseNumber = "DR123456",
                VehicleType = "Van",
                AvailabilityStatus = "Available"
            };

            // Seed booking
            var booking = new Booking
            {
                BookingID = 1,
                CustomerID = "cust-001",
                BookingStatus = "Completed",
                AssignedDriverID = 1,
                Address = "123 Main St",
                ZipCode = "4001",
                City = "Durban",
                Province = "KZN",
                FullName = "Customer",
                Email = "customer@example.com",
                ServiceDate = DateTime.Today,
                ServiceStartTime = DateTime.Today.AddHours(1),
                ServiceEndTime = DateTime.Today.AddHours(2),
                ServiceTypeID = 1
            };

            // Seed dispatch note
            var note = new DispatchNote
            {
                Id = 1,
                BookingID = 1,
                DriverID = 1,
                Note = "Delivered successfully",
                Timestamp = DateTime.UtcNow
            };

            context.Users.Add(driverUser);
            context.DriverDetails.Add(driver);
            context.Booking.Add(booking);
            context.DispatchNotes.Add(note);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetAllDispatchNotes();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var notes = ok.Value as IEnumerable<object>;
            Assert.NotNull(notes);
            Assert.Single(notes);
        }
    }
}