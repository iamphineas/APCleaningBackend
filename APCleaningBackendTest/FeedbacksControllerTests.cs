using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Controllers;
using APCleaningBackend.Model;
using APCleaningBackend.ViewModel;
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
    public class FeedbacksControllerTests
    {
        private const string TestUserId = "user-123";
        private const string CleanerUserId = "cleaner-456";

        private FeedbacksController CreateControllerWithContext(string dbName, out APCleaningBackendContext context)
        {
            var options = new DbContextOptionsBuilder<APCleaningBackendContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .Options;

            context = new APCleaningBackendContext(options);

            var controller = new FeedbacksController(context);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, TestUserId),
                new Claim(ClaimTypes.Role, "Customer")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        [Fact]
        public async Task SubmitFeedback_ValidInput_ReturnsSuccess()
        {
            var controller = CreateControllerWithContext("SubmitFeedbackDb", out var context);

            context.CleanerDetails.Add(new CleanerDetails
            {
                CleanerDetailsID = 1,
                UserId = CleanerUserId,
                CleanerImageUrl = "image.jpg"
            });

            await context.SaveChangesAsync();

            var input = new FeedbackInputModel
            {
                Rating = 5,
                CleanerID = 1,
                Comments = "Great job!"
            };

            var result = await controller.SubmitFeedback(input);
            var ok = Assert.IsType<OkObjectResult>(result);
            var message = ok.Value?.GetType().GetProperty("message")?.GetValue(ok.Value)?.ToString();
            Assert.Equal("Feedback submitted successfully.", message);
        }

        [Fact]
        public async Task SubmitFeedback_InvalidRating_ReturnsBadRequest()
        {
            var controller = CreateControllerWithContext("InvalidRatingDb", out var _);

            var input = new FeedbackInputModel
            {
                Rating = 0,
                CleanerID = 1,
                Comments = "Bad rating"
            };

            var result = await controller.SubmitFeedback(input);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Rating must be between 1 and 5.", badRequest.Value);
        }

        [Fact]
        public async Task GetFeedbackForCleaner_ReturnsList()
        {
            var controller = CreateControllerWithContext("CleanerFeedbackDb", out var context);

            context.CleanerDetails.Add(new CleanerDetails
            {
                CleanerDetailsID = 1,
                UserId = TestUserId,
                CleanerImageUrl = "image.jpg"
            });

            context.Feedback.Add(new Feedback
            {
                FeedbackID = 1,
                CleanerID = 1,
                CustomerID = "cust-001",
                Rating = 4,
                Comments = "Nice",
                CreatedDate = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            var result = await controller.GetFeedbackForCleaner();
            var ok = Assert.IsType<OkObjectResult>(result);
            var feedbackList = Assert.IsAssignableFrom<List<Feedback>>(ok.Value);
            Assert.Single(feedbackList);
        }

        [Fact]
        public async Task GetCleanersWithCompletedBookings_ReturnsList()
        {
            var controller = CreateControllerWithContext("CompletedCleanersDb", out var context);

            context.Users.Add(new ApplicationUser { Id = CleanerUserId, FullName = "Cleaner One" });

            context.CleanerDetails.Add(new CleanerDetails
            {
                CleanerDetailsID = 1,
                UserId = CleanerUserId,
                CleanerImageUrl = "image.jpg"
            });

            context.Booking.Add(new Booking
            {
                BookingID = 1,
                CustomerID = TestUserId,
                BookingStatus = "Completed",
                AssignedCleanerID = 1,
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
            });

            await context.SaveChangesAsync();

            var result = await controller.GetCleanersWithCompletedBookings();
            var ok = Assert.IsType<OkObjectResult>(result);
            var cleaners = ok.Value as IEnumerable<object>;
            Assert.NotNull(cleaners);
            Assert.Single(cleaners);
        }

        [Fact]
        public async Task GetAllFeedback_ReturnsList()
        {
            var controller = CreateControllerWithContext("AllFeedbackDb", out var context);

            context.Users.Add(new ApplicationUser { Id = CleanerUserId, FullName = "Cleaner One" });

            context.CleanerDetails.Add(new CleanerDetails
            {
                CleanerDetailsID = 1,
                UserId = CleanerUserId,
                CleanerImageUrl = "image.jpg"
            });

            context.Feedback.Add(new Feedback
            {
                FeedbackID = 1,
                CleanerID = 1,
                CustomerID = TestUserId,
                Rating = 5,
                Comments = "Excellent",
                CreatedDate = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            var result = await controller.GetAllFeedback();
            var ok = Assert.IsType<OkObjectResult>(result);
            var feedbackList = ok.Value as IEnumerable<object>;
            Assert.NotNull(feedbackList);
            Assert.Single(feedbackList);
        }
    }
}