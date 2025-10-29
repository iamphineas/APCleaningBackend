using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Controllers;
using APCleaningBackend.Model;
using APCleaningBackend.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace APCleaningBackendTest
{
    public class AuthControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<IConfiguration> _mockConfig;

        public AuthControllerTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);

            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["Jwt:Key"]).Returns("supersecurekey1234567890supersecurekey!");
            _mockConfig.Setup(c => c["Frontend:ResetUrl"]).Returns("https://frontend/reset");
            _mockConfig.Setup(c => c["Resend:ApiKey"]).Returns("dummy-api-key");
        }

        private AuthController CreateController(string dbName)
        {
            var options = new DbContextOptionsBuilder<APCleaningBackendContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            var context = new APCleaningBackendContext(options);
            return new AuthController(_mockUserManager.Object, _mockConfig.Object, context);
        }

        private Dictionary<string, string> DeserializePayload(object value)
        {
            var json = JsonSerializer.Serialize(value);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }

        private Dictionary<string, string[]> DeserializeErrorPayload(object value)
        {
            var json = JsonSerializer.Serialize(value);
            return JsonSerializer.Deserialize<Dictionary<string, string[]>>(json);
        }

        // -------------------- Register --------------------
        [Fact]
        public async Task Register_NewUser_ReturnsSuccess()
        {
            var controller = CreateController("RegisterDb");

            var model = new RegisterModel
            {
                Email = "newuser@example.com",
                Password = "Secure123!",
                FullName = "New User",
                PhoneNumber = "0123456789"
            };

            _mockUserManager.Setup(m => m.FindByEmailAsync(model.Email))
                .ReturnsAsync((ApplicationUser)null);
            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Customer"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await controller.Register(model);

            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = DeserializePayload(ok.Value);
            Assert.Equal("User registered successfully", payload["message"]);
        }

        [Fact]
        public async Task Register_ExistingUser_ReturnsBadRequest()
        {
            var controller = CreateController("RegisterExistingDb");
            var model = new RegisterModel { Email = "existing@example.com", Password = "Secure123!" };

            _mockUserManager.Setup(m => m.FindByEmailAsync(model.Email))
                .ReturnsAsync(new ApplicationUser());

            var result = await controller.Register(model);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var payload = DeserializeErrorPayload(bad.Value);
            Assert.Contains("Email is already registered.", payload["errors"]);
        }

        // -------------------- Login --------------------
        [Fact]
        public async Task Login_ValidCredentials_ReturnsJwtToken()
        {
            var controller = CreateController("LoginDb");
            var model = new LoginModel { Email = "user@example.com", Password = "Secure123!" };
            var user = new ApplicationUser { Id = "user-id", Email = model.Email, FullName = "Test User" };

            _mockUserManager.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.CheckPasswordAsync(user, model.Password)).ReturnsAsync(true);
            _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Customer" });

            var result = await controller.Login(model);
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = DeserializePayload(ok.Value);
            Assert.NotNull(payload["token"]);
            Assert.True(payload["token"].Length > 10);
        }

        [Fact]
        public async Task Login_InvalidEmail_ReturnsUnauthorized()
        {
            var controller = CreateController("LoginInvalidEmailDb");
            var model = new LoginModel { Email = "missing@example.com", Password = "Secure123!" };

            _mockUserManager.Setup(m => m.FindByEmailAsync(model.Email))
                .ReturnsAsync((ApplicationUser)null);

            var result = await controller.Login(model);
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var payload = DeserializePayload(unauthorized.Value);
            Assert.Contains("user not found", payload["message"]);
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            var controller = CreateController("LoginInvalidPassDb");
            var model = new LoginModel { Email = "user@example.com", Password = "WrongPass!" };
            var user = new ApplicationUser { Id = "user-id", Email = model.Email };

            _mockUserManager.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.CheckPasswordAsync(user, model.Password)).ReturnsAsync(false);

            var result = await controller.Login(model);
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var payload = DeserializePayload(unauthorized.Value);
            Assert.Contains("incorrect password", payload["message"]);
        }

        // -------------------- Update User --------------------
        [Fact]
        public async Task UpdateProfile_ValidUser_ReturnsSuccess()
        {
            var controller = CreateController("UpdateUserDb");
            var user = new ApplicationUser
            {
                Id = "user-1",
                Email = "old@example.com",
                FullName = "Old User",
                PhoneNumber = "0000"
            };

            var context = new APCleaningBackendContext(
                new DbContextOptionsBuilder<APCleaningBackendContext>()
                .UseInMemoryDatabase("UpdateUserDb").Options);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var model = new UpdateUserModel
            {
                UserId = user.Id,
                FullName = "Updated User",
                Email = "new@example.com",
                PhoneNumber = "9999"
            };

            _mockUserManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var updatedController = new AuthController(_mockUserManager.Object, _mockConfig.Object, context);

            var result = await updatedController.UpdateProfile(model);
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = DeserializePayload(ok.Value);
            Assert.Equal("User updated successfully", payload["message"]);
        }

        // -------------------- Delete Profile --------------------
        [Fact]
        public async Task DeleteProfile_ValidUser_DeletesSuccessfully()
        {
            var controller = CreateController("DeleteUserDb");

            var context = new APCleaningBackendContext(
                new DbContextOptionsBuilder<APCleaningBackendContext>()
                .UseInMemoryDatabase("DeleteUserDb").Options);

            var user = new ApplicationUser { Id = "u1", FullName = "Delete Man", Email = "delete@example.com" };
            context.Users.Add(user);
            context.Booking.Add(new Booking { BookingID = 1, CustomerID = "u1", Address = "1 Delete Street", ZipCode = "5264" });
            await context.SaveChangesAsync();

            _mockUserManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            var deleteController = new AuthController(_mockUserManager.Object, _mockConfig.Object, context);
            var model = new DeleteUserModel { UserId = "u1" };

            var result = await deleteController.DeleteProfile(model);
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = DeserializePayload(ok.Value);
            Assert.Equal("User deleted successfully", payload["message"]);
        }

        // -------------------- Reset Password --------------------
        [Fact]
        public async Task ResetPassword_ValidRequest_ReturnsSuccess()
        {
            var controller = CreateController("ResetPassDb");
            var user = new ApplicationUser { Id = "u1", Email = "reset@example.com" };

            _mockUserManager.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.ResetPasswordAsync(user, "token123", "NewPass!"))
                .ReturnsAsync(IdentityResult.Success);

            var model = new ResetPasswordModel
            {
                Email = user.Email,
                Token = "token123",
                NewPassword = "NewPass!"
            };

            var result = await controller.ResetPassword(model);
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = DeserializePayload(ok.Value);
            Assert.Equal("Password reset successful.", payload["message"]);
        }

        [Fact]
        public async Task ResetPassword_InvalidEmail_ReturnsBadRequest()
        {
            var controller = CreateController("ResetInvalidDb");
            var model = new ResetPasswordModel
            {
                Email = "notfound@example.com",
                Token = "token",
                NewPassword = "Pass123!"
            };

            _mockUserManager.Setup(m => m.FindByEmailAsync(model.Email))
                .ReturnsAsync((ApplicationUser)null);

            var result = await controller.ResetPassword(model);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var payload = DeserializePayload(bad.Value);
            Assert.Equal("Invalid reset request.", payload["message"]);
        }
    }
}


/* Code Attribution 
 * ------------------------------
 * Code by Copilot
 * Link: https://copilot.microsoft.com/shares/CrMRHVUebnaBr1JaX92Rn
 * Accessed: 14 October 2025
 * public async Task Register_NewUser_ReturnsSuccess()
        {
            var model = new RegisterModel
            {
                Email = "newuser@example.com",
                Password = "Secure123!",
                FullName = "New User",
                PhoneNumber = "0123456789"
            };

            _mockUserManager.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync((ApplicationUser)null);
            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Customer"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.Register(model);
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<Dictionary<string, string>>(ok.Value);
            Assert.Equal("User registered successfully", payload["message"]);
        }

        [Fact]
        public async Task Register_ExistingUser_ReturnsBadRequest()
        {
            var model = new RegisterModel { Email = "existing@example.com", Password = "Secure123!" };
            _mockUserManager.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync(new ApplicationUser());

            var result = await _controller.Register(model);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var payload = Assert.IsType<Dictionary<string, string[]>>(badRequest.Value);
            Assert.Contains("Email is already registered.", payload["errors"]);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsJwtToken()
        {
            var model = new LoginModel { Email = "user@example.com", Password = "Secure123!" };
            var user = new ApplicationUser { Id = "user-id", Email = model.Email, FullName = "Test User" };

            _mockUserManager.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.CheckPasswordAsync(user, model.Password)).ReturnsAsync(true);
            _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Customer" });

            var result = await _controller.Login(model);
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<Dictionary<string, string>>(ok.Value);
            Assert.True(payload["token"].Length > 0);
        }

        [Fact]
        public async Task Login_InvalidEmail_ReturnsUnauthorized()
        {
            var model = new LoginModel { Email = "missing@example.com", Password = "Secure123!" };
            _mockUserManager.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync((ApplicationUser)null);

            var result = await _controller.Login(model);
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var payload = Assert.IsType<Dictionary<string, string>>(unauthorized.Value);
            Assert.Contains("user not found", payload["message"]);
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            var model = new LoginModel { Email = "user@example.com", Password = "WrongPass!" };
            var user = new ApplicationUser { Id = "user-id", Email = model.Email };

            _mockUserManager.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.CheckPasswordAsync(user, model.Password)).ReturnsAsync(false);

            var result = await _controller.Login(model);
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var payload = Assert.IsType<Dictionary<string, string>>(unauthorized.Value);
            Assert.Contains("incorrect password", payload["message"]);
        }
    }
 */