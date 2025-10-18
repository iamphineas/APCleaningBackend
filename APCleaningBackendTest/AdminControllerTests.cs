using APCleaningBackend.Controllers;
using APCleaningBackend.Model;
using APCleaningBackend.ViewModel;
using APCleaningBackend.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class AdminControllerTests
{
    private (AdminController controller, APCleaningBackendContext context) CreateControllerWithContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<APCleaningBackendContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .EnableSensitiveDataLogging()
            .Options;

        var context = new APCleaningBackendContext(options);
        var controller = new AdminController(context);
        return (controller, context);
    }

    [Fact]
    public async Task GetBookings_ReturnsList()
    {
        var (controller, context) = CreateControllerWithContext("GetBookingsDb");

        context.ServiceType.Add(new ServiceType
        {
            ServiceTypeID = 1,
            Name = "Standard Clean",
            Description = "Basic cleaning",
            ImageURL = "image.jpg",
            Price = 150
        });

        context.Booking.Add(new Booking
        {
            BookingID = 1,
            FullName = "John Doe",
            Email = "john@example.com",
            Address = "123 Main St",
            City = "Durban",
            Province = "KZN",
            ZipCode = "4001",
            CustomerID = "cust123",
            ServiceTypeID = 1,
            ServiceDate = DateTime.Today,
            BookingAmount = 100
        });

        await context.SaveChangesAsync();

        var result = await controller.GetBookings();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Single(ok.Value as IEnumerable<object>);
    }

    [Fact]
    public async Task AssignBooking_ValidIds_AssignsCleanerAndDriver()
    {
        var (controller, context) = CreateControllerWithContext("AssignBookingDb");

        context.CleanerDetails.Add(new CleanerDetails
        {
            CleanerDetailsID = 1,
            UserId = "c1",
            AvailabilityStatus = "Available"
        });

        context.DriverDetails.Add(new DriverDetails
        {
            DriverDetailsID = 2,
            UserId = "d1",
            AvailabilityStatus = "Available",
            LicenseNumber = "ABC123",
            VehicleType = "Van"
        });

        context.Booking.Add(new Booking
        {
            BookingID = 1,
            FullName = "Jane Doe",
            Email = "jane@example.com",
            Address = "456 Main St",
            City = "Durban",
            Province = "KZN",
            ZipCode = "4001",
            CustomerID = "cust456",
            ServiceTypeID = 1,
            ServiceDate = DateTime.Today,
            BookingAmount = 200
        });

        await context.SaveChangesAsync();

        var model = new BookingAssignmentModel { CleanerID = 1, DriverID = 2 };
        var result = await controller.AssignBooking(1, model);
        var ok = Assert.IsType<OkObjectResult>(result);
        var updated = ok.Value as Booking;
        Assert.Equal("Pending", updated.BookingStatus);
        Assert.Equal(1, updated.AssignedCleanerID);
        Assert.Equal(2, updated.AssignedDriverID);
    }

    [Fact]
    public async Task ResetBookingAssignment_ValidId_ResetsAssignment()
    {
        var (controller, context) = CreateControllerWithContext("ResetBookingDb");

        context.Users.Add(new ApplicationUser
        {
            Id = "c1",
            FullName = "Cleaner One",
            Email = "cleaner@example.com",
            PhoneNumber = "123"
        });

        context.Users.Add(new ApplicationUser
        {
            Id = "d1",
            FullName = "Driver One",
            Email = "driver@example.com",
            PhoneNumber = "456"
        });

        context.Booking.Add(new Booking
        {
            BookingID = 1,
            AssignedCleanerID = 1,
            AssignedDriverID = 2,
            PaymentStatus = "Paid",
            FullName = "Jane Doe",
            Email = "jane@example.com",
            Address = "456 Main St",
            City = "Durban",
            Province = "KZN",
            ZipCode = "4001",
            CustomerID = "cust456",
            ServiceTypeID = 1,
            ServiceDate = DateTime.Today,
            BookingAmount = 200
        });

        context.CleanerDetails.Add(new CleanerDetails
        {
            CleanerDetailsID = 1,
            UserId = "c1",
            AvailabilityStatus = "Unavailable"
        });

        context.DriverDetails.Add(new DriverDetails
        {
            DriverDetailsID = 2,
            UserId = "d1",
            AvailabilityStatus = "Unavailable",
            LicenseNumber = "XYZ789",
            VehicleType = "Truck"
        });

        await context.SaveChangesAsync();

        var result = await controller.ResetBookingAssignment(1);
        var ok = Assert.IsType<OkObjectResult>(result);
        dynamic response = ok.Value;
        Assert.Equal("Confirmed", context.Booking.Find(1).BookingStatus);
        Assert.Null(context.Booking.Find(1).AssignedCleanerID);
        Assert.Null(context.Booking.Find(1).AssignedDriverID);
    }

    [Fact]
    public async Task UpdateBooking_ValidId_UpdatesFields()
    {
        var (controller, context) = CreateControllerWithContext("UpdateBookingDb");

        context.Booking.Add(new Booking
        {
            BookingID = 1,
            FullName = "Old Name",
            Email = "old@example.com",
            Address = "Old St",
            City = "Durban",
            Province = "KZN",
            ZipCode = "4001",
            CustomerID = "custOld",
            ServiceTypeID = 1,
            ServiceDate = DateTime.Today,
            BookingAmount = 100
        });

        await context.SaveChangesAsync();

        var updated = new Booking
        {
            FullName = "New Name",
            Email = "new@example.com",
            Address = "New St",
            City = "Durban",
            Province = "KZN",
            ZipCode = "4001",
            CustomerID = "custNew",
            ServiceTypeID = 1,
            ServiceDate = DateTime.Today,
            ServiceStartTime = DateTime.Today.AddHours(9),
            ServiceEndTime = DateTime.Today.AddHours(11),
            BookingStatus = "Confirmed",
            AssignedCleanerID = 1,
            AssignedDriverID = 2,
            BookingAmount = 250
        };

        var result = await controller.UpdateBooking(1, updated);
        var ok = Assert.IsType<OkObjectResult>(result);
        var booking = context.Booking.Find(1);
        Assert.Equal("New Name", booking.FullName);
        Assert.Equal("custNew", booking.CustomerID);
        Assert.Equal(250, booking.BookingAmount);
    }

    [Fact]
    public async Task DeleteBooking_ValidId_RemovesBooking()
    {
        var (controller, context) = CreateControllerWithContext("DeleteBookingDb");

        context.Booking.Add(new Booking
        {
            BookingID = 1,
            FullName = "Delete Me",
            Email = "delete@example.com",
            Address = "Del St",
            City = "Durban",
            Province = "KZN",
            ZipCode = "4001",
            CustomerID = "custDel",
            ServiceTypeID = 1,
            ServiceDate = DateTime.Today,
            BookingAmount = 50
        });

        await context.SaveChangesAsync();

        var result = await controller.DeleteBooking(1);
        Assert.IsType<NoContentResult>(result);
        Assert.Null(context.Booking.Find(1));
    }

    [Fact]
    public async Task GetCleaners_ReturnsList()
    {
        var (controller, context) = CreateControllerWithContext(Guid.NewGuid().ToString());

        context.Users.Add(new ApplicationUser
        {
            Id = "u1",
            FullName = "Cleaner One",
            Email = "cleaner@example.com",
            PhoneNumber = "123"
        });

        context.ServiceType.Add(new ServiceType
        {
            Name = "Standard Clean",
            Description = "Basic cleaning",
            ImageURL = "image.jpg",
            Price = 150
        });

        await context.SaveChangesAsync();
        var serviceType = await context.ServiceType.FirstAsync();

        context.CleanerDetails.Add(new CleanerDetails
        {
            UserId = "u1",
            ServiceTypeID = serviceType.ServiceTypeID,
            AvailabilityStatus = "Available"
        });

        await context.SaveChangesAsync();

        var result = await controller.GetCleaners();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Single(ok.Value as IEnumerable<object>);
    }

    [Fact]
    public async Task GetDrivers_ReturnsList()
    {
        var (controller, context) = CreateControllerWithContext(Guid.NewGuid().ToString());

        context.Users.Add(new ApplicationUser
        {
            Id = "u1",
            FullName = "Driver One",
            Email = "driver@example.com",
            PhoneNumber = "456"
        });

        context.DriverDetails.Add(new DriverDetails
        {
            UserId = "u1",
            LicenseNumber = "XYZ",
            VehicleType = "Van",
            AvailabilityStatus = "Available"
        });

        await context.SaveChangesAsync();

        var result = await controller.GetDrivers();
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Single(ok.Value as IEnumerable<object>);
    }

    [Fact]
    public async Task GetAnalytics_ReturnsExpectedMetrics()
    {
        var (controller, context) = CreateControllerWithContext("AnalyticsDb");

        // Seed users
        context.Users.AddRange(
            new ApplicationUser { Id = "c1", FullName = "Cleaner One", Email = "cleaner@example.com", PhoneNumber = "123" },
            new ApplicationUser { Id = "d1", FullName = "Driver One", Email = "driver@example.com", PhoneNumber = "456" }
        );

        // Seed service types
        context.ServiceType.AddRange(
            new ServiceType { ServiceTypeID = 1, Name = "Standard Clean", Description = "Basic", ImageURL = "img.jpg", Price = 100 },
            new ServiceType { ServiceTypeID = 2, Name = "Deep Clean", Description = "Thorough", ImageURL = "img2.jpg", Price = 200 }
        );

        // Seed cleaner/driver details
        context.CleanerDetails.Add(new CleanerDetails { CleanerDetailsID = 1, UserId = "c1", AvailabilityStatus = "Available", ServiceTypeID = 1 });
        context.DriverDetails.Add(new DriverDetails { DriverDetailsID = 1, UserId = "d1", AvailabilityStatus = "Available", LicenseNumber = "XYZ", VehicleType = "Van" });

        // Seed bookings
        context.Booking.AddRange(
            new Booking
            {
                BookingID = 1,
                FullName = "Alice",
                Email = "alice@example.com",
                Address = "123 Main St",
                City = "Durban",
                Province = "KZN",
                ZipCode = "4001",
                CustomerID = "cust1",
                ServiceTypeID = 1,
                ServiceDate = DateTime.Today,
                ServiceStartTime = DateTime.Today.AddHours(9),
                ServiceEndTime = DateTime.Today.AddHours(10),
                BookingAmount = 100,
                BookingStatus = "Completed",
                PaymentStatus = "Paid",
                AssignedCleanerID = 1,
                AssignedDriverID = 1
            },
            new Booking
            {
                BookingID = 2,
                FullName = "Bob",
                Email = "bob@example.com",
                Address = "456 Side St",
                City = "Durban",
                Province = "KZN",
                ZipCode = "4001",
                CustomerID = "cust2",
                ServiceTypeID = 2,
                ServiceDate = DateTime.Today.AddDays(-1),
                ServiceStartTime = DateTime.Today.AddHours(11),
                ServiceEndTime = DateTime.Today.AddHours(12),
                BookingAmount = 200,
                BookingStatus = "Pending",
                PaymentStatus = "Paid",
                AssignedCleanerID = null,
                AssignedDriverID = 1
            }
        );

        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetAnalytics();
        var ok = Assert.IsType<OkObjectResult>(result);
        var json = JsonConvert.SerializeObject(ok.Value);
        var data = JObject.Parse(json);

        // Assert key metrics
        Assert.Equal(2, (int)data["totalBookings"]);
        Assert.Equal(300, (int)data["totalRevenue"]);
        Assert.Equal(50.0, (double)data["completionRate"]);
        Assert.Equal(1, (int)data["unassignedCount"]);
        Assert.True(data["bookingsByDay"].HasValues);
        Assert.True(data["revenueByMonth"].HasValues);
        Assert.True(data["cleanerPerformance"].HasValues);
        Assert.True(data["driverEfficiency"].HasValues);
        Assert.True(data["revenueByServiceType"].HasValues);
        Assert.True(data["serviceTypeBreakdown"].HasValues);
        Assert.True(data["peakBookingTimes"].HasValues);
    }

}
