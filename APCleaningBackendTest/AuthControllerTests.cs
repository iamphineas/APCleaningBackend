using APCleaningBackend.Controllers;
using APCleaningBackend.Model;
using APCleaningBackend.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace APCleaningBackendTest
{
    public class AuthControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);

            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["Jwt:Key"]).Returns("supersecurekey1234567890supersecurekey!");

            _controller = new AuthController(_mockUserManager.Object, _mockConfig.Object);
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

        [Fact]
        public async Task Register_NewUser_ReturnsSuccess()
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
            var payload = DeserializePayload(ok.Value);
            Assert.Equal("User registered successfully", payload["message"]);
        }

        [Fact]
        public async Task Register_ExistingUser_ReturnsBadRequest()
        {
            var model = new RegisterModel { Email = "existing@example.com", Password = "Secure123!" };
            _mockUserManager.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync(new ApplicationUser());

            var result = await _controller.Register(model);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var payload = DeserializeErrorPayload(badRequest.Value);
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
            var payload = DeserializePayload(ok.Value);
            Assert.True(payload["token"].Length > 0);
        }

        [Fact]
        public async Task Login_InvalidEmail_ReturnsUnauthorized()
        {
            var model = new LoginModel { Email = "missing@example.com", Password = "Secure123!" };
            _mockUserManager.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync((ApplicationUser)null);

            var result = await _controller.Login(model);
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var payload = DeserializePayload(unauthorized.Value);
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
            var payload = DeserializePayload(unauthorized.Value);
            Assert.Contains("incorrect password", payload["message"]);
        }
    }
}

/* Code Attribution 
 * ------------------------------
 * Code by Copilot
 * Link: t
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