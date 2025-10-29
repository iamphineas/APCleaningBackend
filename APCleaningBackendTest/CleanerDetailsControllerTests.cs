using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Controllers;
using APCleaningBackend.Model;
using APCleaningBackend.Services;
using APCleaningBackend.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace APCleaningBackendTest
{
    public class CleanerDetailsControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<IBlobUploader> _mockBlobUploader;

        public CleanerDetailsControllerTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);

            _mockConfig = new Mock<IConfiguration>();
            _mockBlobUploader = new Mock<IBlobUploader>();

            _mockConfig.Setup(c => c["Azure:CleanerContainer"]).Returns("cleaner-container");
        }

        private (CleanerDetailsController controller, APCleaningBackendContext context) CreateControllerWithContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<APCleaningBackendContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .Options;

            var context = new APCleaningBackendContext(options);

            var controller = new CleanerDetailsController(
                context,
                _mockUserManager.Object,
                _mockConfig.Object,
                _mockBlobUploader.Object
            );

            return (controller, context);
        }

        [Fact]
        public async Task GetCleaners_ReturnsList()
        {
            var (controller, context) = CreateControllerWithContext("GetCleanersDb");

            context.ServiceType.Add(new ServiceType
            {
                ServiceTypeID = 1,
                Name = "Deep Clean",
                Description = "Thorough cleaning",
                ImageURL = "https://example.com/image.jpg"
            });

            var user = new ApplicationUser
            {
                Id = "user-1",
                FullName = "Alwande",
                Email = "alwande@example.com",
                PhoneNumber = "0123456789"
            };

            context.Users.Add(user);
            context.CleanerDetails.Add(new CleanerDetails
            {
                CleanerDetailsID = 101,
                UserId = user.Id,
                ServiceTypeID = 1,
                AvailabilityStatus = "Available",
                CleanerImageUrl = "img.jpg"
            });

            await context.SaveChangesAsync();

            var result = await controller.GetCleaners();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var cleaners = Assert.IsAssignableFrom<IEnumerable<CleanerViewModel>>(ok.Value);
            Assert.Single(cleaners);
        }

        [Fact]
        public async Task GetCleaner_ValidId_ReturnsCleaner()
        {
            var (controller, context) = CreateControllerWithContext("GetCleanerDb");

            var user = new ApplicationUser
            {
                Id = "user-2",
                FullName = "Alwande",
                Email = "alwande@example.com",
                PhoneNumber = "0123456789"
            };

            context.Users.Add(user);
            context.CleanerDetails.Add(new CleanerDetails
            {
                CleanerDetailsID = 1,
                UserId = user.Id,
                ServiceTypeID = 1,
                AvailabilityStatus = "Available",
                CleanerImageUrl = "img2.jpg"
            });

            await context.SaveChangesAsync();

            var result = await controller.GetCleaner(1);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var cleaner = Assert.IsType<CleanerUpdateViewModel>(ok.Value);
            Assert.Equal("Alwande", cleaner.FullName);
        }

        [Fact]
        public async Task PostCleaner_ValidModel_CreatesCleaner()
        {
            var (controller, context) = CreateControllerWithContext("PostCleanerDb");

            var service = new ServiceType
            {
                ServiceTypeID = 1,
                Name = "Deep Clean",
                Description = "Thorough cleaning",
                ImageURL = "https://example.com/image.jpg"
            };

            context.ServiceType.Add(service);
            await context.SaveChangesAsync();

            var mockFile = new Mock<IFormFile>();
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write("fake image content");
            writer.Flush();
            ms.Position = 0;
            mockFile.Setup(f => f.OpenReadStream()).Returns(ms);
            mockFile.Setup(f => f.FileName).Returns("cleaner.jpg");
            mockFile.Setup(f => f.Length).Returns(ms.Length);

            var model = new CleanerRegisterModel
            {
                FullName = "Alwande",
                Email = "alwande@example.com",
                PhoneNumber = "0123456789",
                Password = "Secure123!",
                ServiceTypeID = 1,
                CleanerImage = mockFile.Object
            };

            _mockBlobUploader.Setup(b => b.UploadAsync(model.CleanerImage, "cleaner-container"))
                .ReturnsAsync("uploaded-cleaner.jpg");

            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Cleaner"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await controller.PostCleaner(model);
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var cleaner = Assert.IsType<CleanerDetails>(created.Value);
            Assert.Equal("Available", cleaner.AvailabilityStatus);
            Assert.Equal("uploaded-cleaner.jpg", cleaner.CleanerImageUrl);
        }

        [Fact]
        public async Task DeleteCleaner_ValidId_RemovesCleaner()
        {
            var (controller, context) = CreateControllerWithContext("DeleteCleanerDb");

            var user = new ApplicationUser
            {
                Id = "user-3",
                Email = "delete@example.com",
                FullName = "Delete Me"
            };

            context.Users.Add(user);
            context.CleanerDetails.Add(new CleanerDetails
            {
                CleanerDetailsID = 1,
                UserId = user.Id,
                ServiceTypeID = 1,
                AvailabilityStatus = "Available",
                CleanerImageUrl = "test-cleaner.jpg"
            });

            await context.SaveChangesAsync();

            var result = await controller.DeleteCleaner(1);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateCleaner_ValidId_UpdatesCleaner()
        {
            var (controller, context) = CreateControllerWithContext("UpdateCleanerDb");

            var user = new ApplicationUser
            {
                Id = "user-4",
                FullName = "Old Name",
                Email = "old@example.com",
                PhoneNumber = "0000000000"
            };

            context.Users.Add(user);
            context.ServiceType.Add(new ServiceType
            {
                ServiceTypeID = 1,
                Name = "Basic Clean",
                Description = "Basic cleaning",
                ImageURL = "https://example.com/basic.jpg"
            });

            context.CleanerDetails.Add(new CleanerDetails
            {
                CleanerDetailsID = 1,
                UserId = user.Id,
                ServiceTypeID = 1,
                AvailabilityStatus = "Busy",
                CleanerImageUrl = "old.jpg"
            });

            await context.SaveChangesAsync();

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("updated.jpg");
            mockFile.Setup(f => f.Length).Returns(100);

            _mockBlobUploader.Setup(b => b.UploadAsync(It.IsAny<IFormFile>(), "cleaner-container"))
                .ReturnsAsync("new-uploaded.jpg");

            var model = new CleanerUpdateModel
            {
                FullName = "Updated Name",
                Email = "updated@example.com",
                PhoneNumber = "9999999999",
                ServiceTypeID = 1,
                AvailabilityStatus = "Available",
                CleanerImage = mockFile.Object
            };

            var result = await controller.UpdateCleaner(1, model);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Cleaner updated successfully.", ok.Value);
        }

        [Fact]
        public async Task ResetCleanerPassword_ValidId_ResetsPassword()
        {
            var (controller, context) = CreateControllerWithContext("ResetCleanerPasswordDb");

            var user = new ApplicationUser
            {
                Id = "user-5",
                Email = "reset@example.com",
                FullName = "Reset Me"
            };

            context.Users.Add(user);
            context.CleanerDetails.Add(new CleanerDetails
            {
                CleanerDetailsID = 1,
                UserId = user.Id,
                ServiceTypeID = 1,
                AvailabilityStatus = "Available",
                CleanerImageUrl = "test-cleaner.jpg"
            });

            await context.SaveChangesAsync();

            _mockUserManager.Setup(m => m.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");
            _mockUserManager.Setup(m => m.ResetPasswordAsync(user, "reset-token", "NewPass123!"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await controller.ResetCleanerPassword(1, "NewPass123!");
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Password reset successfully.", ok.Value);
        }
    }
}


/* Code Attribution 
 * ------------------------------
 * Code by Copilot
 * Link: t
 * Accessed: 14 October 2025
 * using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Controllers;
using APCleaningBackend.Model;
using APCleaningBackend.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace APCleaningBackendTest
{
    public class CleanerDetailsControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

        public CleanerDetailsControllerTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        private (CleanerDetailsController controller, APCleaningBackendContext context) CreateControllerWithContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<APCleaningBackendContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .Options;

            var context = new APCleaningBackendContext(options);
            var controller = new CleanerDetailsController(context, _mockUserManager.Object);
            return (controller, context);
        }

        [Fact]
        public async Task GetCleaners_ReturnsList()
        {
            var (controller, context) = CreateControllerWithContext("GetCleanersDb");

            context.ServiceType.Add(new ServiceType
            {
                ServiceTypeID = 1,
                Name = "Deep Clean",
                Description = "Thorough cleaning",
                ImageURL = "https://example.com/image.jpg"
            });

            var user = new ApplicationUser
            {
                Id = "user-1",
                FullName = "Alwande",
                Email = "alwande@example.com",
                PhoneNumber = "0123456789"
            };

            context.Users.Add(user);
            context.CleanerDetails.Add(new CleanerDetails
            {
                CleanerDetailsID = 101,
                UserId = user.Id,
                ServiceTypeID = 1,
                AvailabilityStatus = "Available"
            });

            await context.SaveChangesAsync();

            var result = await controller.GetCleaners();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var cleaners = Assert.IsAssignableFrom<IEnumerable<CleanerViewModel>>(ok.Value);
            Assert.Single(cleaners);
        }

        [Fact]
        public async Task GetCleaner_ValidId_ReturnsCleaner()
        {
            var (controller, context) = CreateControllerWithContext("GetCleanerDb");

            var user = new ApplicationUser
            {
                Id = "user-2",
                FullName = "Alwande",
                Email = "alwande@example.com",
                PhoneNumber = "0123456789"
            };

            context.Users.Add(user);
            context.CleanerDetails.Add(new CleanerDetails
            {
                CleanerDetailsID = 1,
                UserId = user.Id,
                ServiceTypeID = 1,
                AvailabilityStatus = "Available"
            });

            await context.SaveChangesAsync();

            var result = await controller.GetCleaner(1);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var cleaner = Assert.IsType<CleanerRegisterModel>(ok.Value);
            Assert.Equal("Alwande", cleaner.FullName);
        }

        [Fact]
        public async Task PostCleaner_ValidModel_CreatesCleaner()
        {
            var (controller, context) = CreateControllerWithContext("PostCleanerDb");

            var model = new CleanerRegisterModel
            {
                FullName = "Alwande",
                Email = "alwande@example.com",
                PhoneNumber = "0123456789",
                Password = "Secure123!",
                ServiceTypeID = 1
            };

            context.ServiceType.Add(new ServiceType
            {
                ServiceTypeID = 1,
                Name = "Deep Clean",
                Description = "Thorough cleaning",
                ImageURL = "https://example.com/image.jpg"
            });

            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Cleaner"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await controller.PostCleaner(model);
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var cleaner = Assert.IsType<CleanerDetails>(created.Value);
            Assert.Equal("Available", cleaner.AvailabilityStatus);
        }

        [Fact]
        public async Task DeleteCleaner_ValidId_RemovesCleaner()
        {
            var (controller, context) = CreateControllerWithContext("DeleteCleanerDb");

            var user = new ApplicationUser { Id = "user-3", Email = "delete@example.com" };
            context.Users.Add(user);
            context.CleanerDetails.Add(new CleanerDetails
            {
                CleanerDetailsID = 1,
                UserId = user.Id,
                ServiceTypeID = 1,
                AvailabilityStatus = "Available"
            });

            await context.SaveChangesAsync();

            var result = await controller.DeleteCleaner(1);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateCleaner_ValidId_UpdatesCleaner()
        {
            var (controller, context) = CreateControllerWithContext("UpdateCleanerDb");

            var user = new ApplicationUser
            {
                Id = "user-4",
                FullName = "Old Name",
                Email = "old@example.com",
                PhoneNumber = "0000000000"
            };

            context.Users.Add(user);
            context.ServiceType.Add(new ServiceType
            {
                ServiceTypeID = 1,
                Name = "Basic Clean",
                Description = "Basic cleaning",
                ImageURL = "https://example.com/basic.jpg"
            });

            context.CleanerDetails.Add(new CleanerDetails
            {
                CleanerDetailsID = 1,
                UserId = user.Id,
                ServiceTypeID = 1,
                AvailabilityStatus = "Busy"
            });

            await context.SaveChangesAsync();

            var model = new CleanerUpdateModel
            {
                FullName = "Updated Name",
                Email = "updated@example.com",
                PhoneNumber = "9999999999",
                ServiceTypeID = 1,
                AvailabilityStatus = "Available"
            };

            var result = await controller.UpdateCleaner(1, model);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Cleaner updated successfully.", ok.Value);
        }

        [Fact]
        public async Task ResetCleanerPassword_ValidId_ResetsPassword()
        {
            var (controller, context) = CreateControllerWithContext("ResetPasswordDb");

            var user = new ApplicationUser { Id = "user-5", Email = "reset@example.com" };
            context.Users.Add(user);
            context.CleanerDetails.Add(new CleanerDetails
            {
                CleanerDetailsID = 1,
                UserId = user.Id,
                ServiceTypeID = 1,
                AvailabilityStatus = "Available"
            });

            await context.SaveChangesAsync();

            _mockUserManager.Setup(m => m.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");
            _mockUserManager.Setup(m => m.ResetPasswordAsync(user, "reset-token", "NewPass123!"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await controller.ResetCleanerPassword(1, "NewPass123!");
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Password reset successfully.", ok.Value);
        }
    }
}
 */