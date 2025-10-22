using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Model;
using APCleaningBackend.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Resend;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace APCleaningBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly APCleaningBackendContext _context;


        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration config, APCleaningBackendContext context)
        {
            _userManager = userManager;
            _config = config;
            _context = context;

        }
        // GET: AuthController
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest(new { errors = new[] { "Email is already registered." } });
            }



            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { errors });
            }

            await _userManager.AddToRoleAsync(user, "Customer");

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            Console.WriteLine($"Login attempt for: {model.Email}");

            var user = await _userManager.FindByEmailAsync(model.Email);
            Console.WriteLine($"User found: {user != null}");

            if (user == null) return Unauthorized(new { message = "Invalid credentials: user not found" });

            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);

            Console.WriteLine($"Password check: {passwordValid}");



            if (!passwordValid) return Unauthorized(new { message = "Invalid credentials: incorrect password" });

            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault() ?? "Customer";


            // create claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("fullName", user.FullName ?? ""),
                new Claim(ClaimTypes.Role, userRole)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest(new { message = "No account found with that email." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"{_config["Frontend:ResetUrl"]}?email={user.Email}&token={Uri.EscapeDataString(token)}";

            IResend resend = ResendClient.Create(_config["Resend:ApiKey"]);

            var email = new EmailMessage
            {
                From = "onboarding@resend.dev",
                To = "alwandengcobo3@gmail.com",
                Subject = "Reset Your AP Cleaning Password",
                HtmlBody = $@"
            <p>Hello {user.FullName ?? "there"},</p>
            <p>Click the link below to reset your password:</p>
            <p><a href='{resetLink}' style='color:#392C3A;'>Reset Password</a></p>
            <p>If you didn’t request this, you can safely ignore it.</p>"
            };

            var result = await resend.EmailSendAsync(email);

            if (!result.Success)
                return StatusCode(500, new { message = "Failed to send email." });

            return Ok(new { message = "Reset link sent successfully." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest(new { message = "Invalid reset request." });

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { errors });
            }

            return Ok(new { message = "Password reset successful." });
        }

        [HttpPost("updateUser")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserModel model)
        {
            var existingUser = await _userManager.FindByIdAsync(model.UserId);
            if (existingUser == null) throw new Exception("User not found");

            existingUser.UserName = model.FullName;
            existingUser.Email = model.Email;
            existingUser.FullName = model.FullName;
            existingUser.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(existingUser);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { errors });
            }


            return Ok(new { message = "User updated successfully" });
        }

        [HttpPost("deleteProfile")]
        public async Task<IActionResult> DeleteProfile([FromBody] DeleteUserModel model)
        {
            var existingUser = await _userManager.FindByIdAsync(model.UserId);
            if (existingUser == null) throw new Exception("User not found");

            var userBookings = await _context.Booking
                .Where(b => b.CustomerID == model.UserId) 
                .ToListAsync();

            _context.Booking.RemoveRange(userBookings);


            var result = await _userManager.DeleteAsync(existingUser);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { errors });
            }

            return Ok(new { message = "User deleted successfully" });
        }
    }
}
