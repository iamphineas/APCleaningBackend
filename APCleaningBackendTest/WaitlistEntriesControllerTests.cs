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
using System.Threading.Tasks;
using Xunit;

namespace APCleaningBackendTest
{
    public class WaitlistEntriesControllerTests
    {
        private readonly Mock<IEmailService> _mockEmailService = new();

        private WaitlistEntriesController CreateControllerWithContext(string dbName, out APCleaningBackendContext context)
        {
            var options = new DbContextOptionsBuilder<APCleaningBackendContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .Options;

            context = new APCleaningBackendContext(options);
            return new WaitlistEntriesController(context, _mockEmailService.Object);
        }

        [Fact]
        public async Task SubmitEmail_ValidEmail_SavesAndSendsConfirmation()
        {
            var controller = CreateControllerWithContext("ValidEmailDb", out var context);

            var entry = new WaitlistEntry { Email = "test@example.com" };
            var result = await controller.SubmitEmail(entry);

            var ok = Assert.IsType<OkObjectResult>(result);
            var message = ok.Value?.GetType().GetProperty("message")?.GetValue(ok.Value)?.ToString();
            Assert.Equal("Email saved", message);

            var saved = await context.WaitlistEntry.FirstOrDefaultAsync(e => e.Email == "test@example.com");
            Assert.NotNull(saved);

            _mockEmailService.Verify(s => s.SendWaitlistConfirmationAsync("test@example.com"), Times.Once);
        }

        [Fact]
        public async Task SubmitEmail_DuplicateEmail_ReturnsConflict()
        {
            var controller = CreateControllerWithContext("DuplicateEmailDb", out var context);

            context.WaitlistEntry.Add(new WaitlistEntry
            {
                Email = "duplicate@example.com",
                SubmittedAt = DateTime.UtcNow.AddDays(-1)
            });
            await context.SaveChangesAsync();

            var entry = new WaitlistEntry { Email = "duplicate@example.com" };
            var result = await controller.SubmitEmail(entry);

            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("Email already on the waitlist.", conflict.Value);
        }

        [Fact]
        public async Task SubmitEmail_EmptyEmail_ReturnsBadRequest()
        {
            var controller = CreateControllerWithContext("EmptyEmailDb", out var _);

            var entry = new WaitlistEntry { Email = "   " };
            var result = await controller.SubmitEmail(entry);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Email is required.", badRequest.Value);
        }

        [Fact]
        public async Task GetWaitlist_ReturnsEntries()
        {
            var controller = CreateControllerWithContext("GetWaitlistDb", out var context);

            context.WaitlistEntry.AddRange(new List<WaitlistEntry>
            {
                new WaitlistEntry { Email = "one@example.com", SubmittedAt = DateTime.UtcNow.AddMinutes(-10) },
                new WaitlistEntry { Email = "two@example.com", SubmittedAt = DateTime.UtcNow }
            });

            await context.SaveChangesAsync();

            var result = await controller.GetWaitlist();
            var ok = Assert.IsType<OkObjectResult>(result);
            var entries = Assert.IsAssignableFrom<List<WaitlistEntry>>(ok.Value);
            Assert.Equal(2, entries.Count);
            Assert.Equal("two@example.com", entries[0].Email);
        }
    }
}