using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Model;
using Microsoft.AspNetCore.Identity;
using APCleaningBackend.ViewModel;
using APCleaningBackend.Services;

namespace APCleaningBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriverDetailsController : ControllerBase
    {
        private readonly APCleaningBackendContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBlobUploader _blobUploader;
        private readonly string _driverContainer;

        public DriverDetailsController(APCleaningBackendContext context, UserManager<ApplicationUser> userManager, IConfiguration config, IBlobUploader blobUploader)
        {
            _context = context;
            _userManager = userManager;
            _blobUploader = blobUploader;
            _driverContainer = config["Azure:DriverContainer"];
        }

        // GET: DriverDetails
        [HttpGet]
        public async Task<IActionResult> GetDrivers()
        {
            var drivers = await (from dd in _context.DriverDetails
                                 join user in _context.Users
                                 on dd.UserId equals user.Id
                                 select new DriverViewModel
                                 {
                                     DriverDetailsID = dd.DriverDetailsID,
                                     FullName = user.FullName,
                                     Email = user.Email,
                                     PhoneNumber = user.PhoneNumber,
                                     LicenseNumber = dd.LicenseNumber,
                                     VehicleType = dd.VehicleType,
                                     AvailabilityStatus = dd.AvailabilityStatus,
                                     DriverImageUrl = dd.DriverImageUrl,
                                 }).ToListAsync();

            return Ok(drivers);

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDriver(int id)
        {
            var driver = await (from dd in _context.DriverDetails
                                join user in _context.Users
                                on dd.UserId equals user.Id
                                where dd.DriverDetailsID == id
                                select new DriverUpdateViewModel
                                {
                                    FullName = user.FullName,
                                    Email = user.Email,
                                    PhoneNumber = user.PhoneNumber,
                                    LicenseNumber = dd.LicenseNumber,
                                    VehicleType = dd.VehicleType,
                                    AvailabilityStatus = dd.AvailabilityStatus,
                                    DriverImageUrl = dd.DriverImageUrl
                                }).FirstOrDefaultAsync();

            if (driver == null)
                return NotFound();

            return Ok(driver);
        }

        // POST: api/DriverDetails
        [HttpPost]
        public async Task<ActionResult<DriverDetails>> PostDriver([FromForm] DriverRegisterModel model)
        {
            string uploadedFileName = null;

            try
            {
                uploadedFileName = await _blobUploader.UploadAsync(model.DriverImage, _driverContainer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Blob upload failed: {ex.Message}");
                return StatusCode(500, "File upload failed.");
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PasswordHash = model.Password,
                PhoneNumber = model.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Assign "Driver" role
            await _userManager.AddToRoleAsync(user, "Driver");

            try
            {
                // Create DriverDetails
                var driver = new DriverDetails
                {
                    UserId = user.Id,
                    LicenseNumber = model.LicenseNumber,
                    AvailabilityStatus = "Available",
                    VehicleType = model.VehicleType,
                    DriverImageUrl = uploadedFileName
                };

                _context.DriverDetails.Add(driver);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDriver), new { id = driver.DriverDetailsID }, driver);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDriver(int id)
        {
            var driver = await _context.DriverDetails.FindAsync(id);
            if (driver == null)
                return NotFound();

            var user = await _context.Users.FindAsync(driver.UserId);
            if (user != null)
                _context.Users.Remove(user);

            _context.DriverDetails.Remove(driver);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDriver(int id, [FromForm] DriverUpdateModel model)
        {
            var driver = await _context.DriverDetails.FindAsync(id);
            if (driver == null)
                return NotFound("Driver record not found.");

            var user = await _context.Users.FindAsync(driver.UserId);
            if (user == null)
                return NotFound("User profile not found.");

            string uploadedFileName = driver.DriverImageUrl;

            if (model.DriverImage != null && model.DriverImage.Length > 0)
            {
                try
                {
                    uploadedFileName = await _blobUploader.UploadAsync(model.DriverImage, _driverContainer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Blob upload failed: {ex.Message}");
                    return StatusCode(500, "File upload failed.");
                }
            }

            // Update user profile
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            // Update driver details
            driver.LicenseNumber = model.LicenseNumber;
            driver.VehicleType = model.VehicleType;
            driver.AvailabilityStatus = model.AvailabilityStatus;
            driver.DriverImageUrl = uploadedFileName;

            Console.WriteLine("Incoming payload:");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(model));

            _context.Users.Update(user);
            _context.DriverDetails.Update(driver);
            await _context.SaveChangesAsync();

            return Ok("Driver updated successfully.");
        }

        [HttpPut("reset-password/{driverId}")]
        public async Task<IActionResult> ResetDriverPassword(int driverId, [FromBody] string newPassword)
        {
            var driver = await _context.DriverDetails.FindAsync(driverId);
            if (driver == null)
                return NotFound("Driver not found.");

            var user = await _context.Users.FindAsync(driver.UserId);
            if (user == null)
                return NotFound("User profile not found.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Password reset successfully.");
        }
    }
}
