using APCleaningBackend.Areas.Identity.Data;
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
    public class DriverDetailsControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

        public DriverDetailsControllerTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        private (DriverDetailsController controller, APCleaningBackendContext context) CreateControllerWithContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<APCleaningBackendContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .Options;

            var context = new APCleaningBackendContext(options);
            var controller = new DriverDetailsController(context, _mockUserManager.Object);
            return (controller, context);
        }

        [Fact]
        public async Task GetDrivers_ReturnsList()
        {
            var (controller, context) = CreateControllerWithContext("GetDriversDb");

            var user = new ApplicationUser
            {
                Id = "user-1",
                FullName = "Alwande",
                Email = "alwande@example.com",
                PhoneNumber = "0123456789"
            };

            context.Users.Add(user);
            context.DriverDetails.Add(new DriverDetails
            {
                DriverDetailsID = 1,
                UserId = user.Id,
                LicenseNumber = "DR123456",
                VehicleType = "Van",
                AvailabilityStatus = "Available"
            });

            await context.SaveChangesAsync();

            var result = await controller.GetDrivers();
            var ok = Assert.IsType<OkObjectResult>(result);
            var drivers = Assert.IsAssignableFrom<IEnumerable<DriverViewModel>>(ok.Value);
            Assert.Single(drivers);
        }

        [Fact]
        public async Task GetDriver_ValidId_ReturnsDriver()
        {
            var (controller, context) = CreateControllerWithContext("GetDriverDb");

            var user = new ApplicationUser
            {
                Id = "user-2",
                FullName = "Alwande",
                Email = "alwande@example.com",
                PhoneNumber = "0123456789"
            };

            context.Users.Add(user);
            context.DriverDetails.Add(new DriverDetails
            {
                DriverDetailsID = 1,
                UserId = user.Id,
                LicenseNumber = "DR123456",
                VehicleType = "Van",
                AvailabilityStatus = "Available"
            });

            await context.SaveChangesAsync();

            var result = await controller.GetDriver(1);
            var ok = Assert.IsType<OkObjectResult>(result);
            var driver = Assert.IsType<DriverUpdateModel>(ok.Value);
            Assert.Equal("Alwande", driver.FullName);
        }

        [Fact]
        public async Task PostDriver_ValidModel_CreatesDriver()
        {
            var (controller, context) = CreateControllerWithContext("PostDriverDb");

            var model = new DriverViewModel
            {
                FullName = "Alwande",
                Email = "alwande@example.com",
                PhoneNumber = "0123456789",
                Password = "Secure123!",
                LicenseNumber = "DR123456",
                VehicleType = "Van"
            };

            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Driver"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await controller.PostDriver(model);
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var driver = Assert.IsType<DriverDetails>(created.Value);
            Assert.Equal("Available", driver.AvailabilityStatus);
        }

        [Fact]
        public async Task DeleteDriver_ValidId_RemovesDriver()
        {
            var (controller, context) = CreateControllerWithContext("DeleteDriverDb");

            var user = new ApplicationUser
            {
                Id = "user-3",
                Email = "delete@example.com",
                FullName = "Delete Me"
            };

            context.Users.Add(user);
            context.DriverDetails.Add(new DriverDetails
            {
                DriverDetailsID = 1,
                UserId = user.Id,
                LicenseNumber = "DR123456",
                VehicleType = "Van",
                AvailabilityStatus = "Available"
            });

            await context.SaveChangesAsync();

            var result = await controller.DeleteDriver(1);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateDriver_ValidId_UpdatesDriver()
        {
            var (controller, context) = CreateControllerWithContext("UpdateDriverDb");

            var user = new ApplicationUser
            {
                Id = "user-4",
                FullName = "Old Name",
                Email = "old@example.com",
                PhoneNumber = "0000000000"
            };

            context.Users.Add(user);
            context.DriverDetails.Add(new DriverDetails
            {
                DriverDetailsID = 1,
                UserId = user.Id,
                LicenseNumber = "DR123456",
                VehicleType = "Van",
                AvailabilityStatus = "Busy"
            });

            await context.SaveChangesAsync();

            var model = new DriverUpdateModel
            {
                FullName = "Updated Name",
                Email = "updated@example.com",
                PhoneNumber = "9999999999",
                LicenseNumber = "DR654321",
                VehicleType = "Truck",
                AvailabilityStatus = "Available"
            };

            var result = await controller.UpdateDriver(1, model);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Driver updated successfully.", ok.Value);
        }

        [Fact]
        public async Task ResetDriverPassword_ValidId_ResetsPassword()
        {
            var (controller, context) = CreateControllerWithContext("ResetDriverPasswordDb");

            var user = new ApplicationUser
            {
                Id = "user-5",
                Email = "reset@example.com",
                FullName = "Reset Me"
            };

            context.Users.Add(user);
            context.DriverDetails.Add(new DriverDetails
            {
                DriverDetailsID = 1,
                UserId = user.Id,
                LicenseNumber = "DR123456",
                VehicleType = "Van",
                AvailabilityStatus = "Available"
            });

            await context.SaveChangesAsync();

            _mockUserManager.Setup(m => m.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");
            _mockUserManager.Setup(m => m.ResetPasswordAsync(user, "reset-token", "NewPass123!"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await controller.ResetDriverPassword(1, "NewPass123!");
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Password reset successfully.", ok.Value);
        }
    }
}