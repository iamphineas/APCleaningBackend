using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Controllers;
using APCleaningBackend.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace APCleaningBackendTest
{
    public class ContactsControllerTests
    {
        private ContactsController CreateControllerWithContext(string dbName, out APCleaningBackendContext context)
        {
            var options = new DbContextOptionsBuilder<APCleaningBackendContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .Options;

            context = new APCleaningBackendContext(options);
            return new ContactsController(context);
        }

        [Fact]
        public async Task GetContact_ReturnsAllContacts()
        {
            var controller = CreateControllerWithContext("GetContactsDb", out var context);

            context.Contact.Add(new Contact
            {
                ContactID = 1,
                Name = "Alice",
                Email = "alice@example.com",
                Subject = "Question",
                Message = "How do I book?",
                IsResolved = false
            });

            await context.SaveChangesAsync();

            var result = await controller.GetContact();
            var ok = Assert.IsType<ActionResult<IEnumerable<Contact>>>(result);
            var contacts = Assert.IsAssignableFrom<List<Contact>>(ok.Value);
            Assert.Single(contacts);
        }

        [Fact]
        public async Task PostContact_ValidModel_CreatesContact()
        {
            var controller = CreateControllerWithContext("PostContactDb", out var context);

            var model = new Contact
            {
                Name = "Bob",
                Email = "bob@example.com",
                Subject = "Help",
                Message = "Need assistance"
            };

            var result = await controller.PostContact(model);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var contact = Assert.IsType<Contact>(ok.Value);
            Assert.Equal("Bob", contact.Name);
            Assert.False(contact.IsResolved);
        }

        [Fact]
        public async Task PostContact_MissingFields_ReturnsBadRequest()
        {
            var controller = CreateControllerWithContext("MissingFieldsDb", out var _);

            var model = new Contact
            {
                Name = "",
                Email = "",
                Subject = "",
                Message = ""
            };

            var result = await controller.PostContact(model);
            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("All fields are required.", bad.Value);
        }

        [Fact]
        public async Task PostContact_InvalidEmail_ReturnsBadRequest()
        {
            var controller = CreateControllerWithContext("InvalidEmailDb", out var _);

            var model = new Contact
            {
                Name = "Charlie",
                Email = "invalid-email",
                Subject = "Issue",
                Message = "Something's wrong"
            };

            var result = await controller.PostContact(model);
            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Invalid email format.", bad.Value);
        }

        [Fact]
        public async Task MarkResolved_ValidId_SetsIsResolvedTrue()
        {
            var controller = CreateControllerWithContext("ResolveContactDb", out var context);

            context.Contact.Add(new Contact
            {
                ContactID = 1,
                Name = "Dana",
                Email = "dana@example.com",
                Subject = "Feedback",
                Message = "Great service",
                IsResolved = false
            });

            await context.SaveChangesAsync();

            var result = await controller.MarkResolved(1);
            Assert.IsType<NoContentResult>(result);

            var updated = await context.Contact.FindAsync(1);
            Assert.True(updated.IsResolved);
        }

        [Fact]
        public async Task MarkResolved_InvalidId_ReturnsNotFound()
        {
            var controller = CreateControllerWithContext("ResolveInvalidDb", out var _);
            var result = await controller.MarkResolved(999);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}