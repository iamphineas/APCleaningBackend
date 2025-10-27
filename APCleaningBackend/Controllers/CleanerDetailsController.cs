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
using Azure.Storage.Blobs;
using APCleaningBackend.ViewModel;
using APCleaningBackend.Services;

namespace APCleaningBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CleanerDetailsController : ControllerBase
    {
        private readonly APCleaningBackendContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBlobUploader _blobUploader;
        private readonly string _cleanerContainer;

        public CleanerDetailsController(APCleaningBackendContext context, UserManager<ApplicationUser> userManager, IConfiguration config, IBlobUploader blobUploader)
        {
            _context = context;
            _userManager = userManager;
            _blobUploader = blobUploader;
            _cleanerContainer = config["Azure:CleanerContainer"];
        }

        // GET: CleanerDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CleanerViewModel>>> GetCleaners()
        {
            var cleaners = await (from cd in _context.CleanerDetails
                                  join user in _context.Users
                                  on cd.UserId equals user.Id
                                  join service in _context.ServiceType
                                  on cd.ServiceTypeID equals service.ServiceTypeID
                                  select new CleanerViewModel
                                  {
                                      CleanerDetailsID = cd.CleanerDetailsID,
                                      FullName = user.FullName,
                                      Email = user.Email,
                                      PhoneNumber = user.PhoneNumber,
                                      ServiceTypeID = cd.ServiceTypeID,
                                      ServiceName = service.Name,
                                      AvailabilityStatus = cd.AvailabilityStatus,
                                      CleanerImageUrl = cd.CleanerImageUrl,
                                  }).ToListAsync();

            return Ok(cleaners);
        }

        // GET: api/CleanerDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CleanerDetails>> GetCleaner(int id)
        {
            var driver = await (from cd in _context.CleanerDetails
                                join user in _context.Users
                                on cd.UserId equals user.Id
                                where cd.CleanerDetailsID == id
                                select new CleanerUpdateViewModel
                                {
                                    FullName = user.FullName,
                                    Email = user.Email,
                                    PhoneNumber = user.PhoneNumber,
                                    ServiceTypeID= cd.ServiceTypeID,
                                    AvailabilityStatus = cd.AvailabilityStatus,
                                    CleanerImageUrl = cd.CleanerImageUrl,
                                }).FirstOrDefaultAsync();

            if (driver == null)
                return NotFound();

            return Ok(driver);
        }

        // POST: api/CleanerDetails
        [HttpPost]
        public async Task<ActionResult<CleanerDetails>> PostCleaner([FromForm] CleanerRegisterModel model)
        {
            string uploadedFileName = null;

            try
            {
                uploadedFileName = await _blobUploader.UploadAsync(model.CleanerImage, _cleanerContainer);
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

            // Assign "Cleaner" role
            await _userManager.AddToRoleAsync(user, "Cleaner");



            try
            {
                // Create CleanerDetails
                var cleaner = new CleanerDetails
                {
                    UserId = user.Id,
                    ServiceTypeID = model.ServiceTypeID,
                    AvailabilityStatus = "Available",
                    CleanerImageUrl = uploadedFileName
                };

                _context.CleanerDetails.Add(cleaner);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCleaner), new { id = cleaner.CleanerDetailsID }, cleaner);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCleaner(int id)
        {
            var cleaner = await _context.CleanerDetails.FindAsync(id);
            if (cleaner == null)
                return NotFound();

            var user = await _context.Users.FindAsync(cleaner.UserId);
            if (user != null)
                _context.Users.Remove(user);

            _context.CleanerDetails.Remove(cleaner);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCleaner(int id, [FromForm] CleanerUpdateModel model)
        {
            string uploadedFileName = null;

            try
            {
                uploadedFileName = await _blobUploader.UploadAsync(model.CleanerImage, _cleanerContainer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Blob upload failed: {ex.Message}");
                return StatusCode(500, "File upload failed.");
            }

            var cleaner = await _context.CleanerDetails.FindAsync(id);
            if (cleaner == null)
                return NotFound("Driver record not found.");

            var user = await _context.Users.FindAsync(cleaner.UserId);
            if (user == null)
                return NotFound("User profile not found.");

            // Update user profile
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            // Update cleaner details
            cleaner.ServiceTypeID = model.ServiceTypeID;
            cleaner.AvailabilityStatus = model.AvailabilityStatus;
            cleaner.CleanerImageUrl = uploadedFileName;

            Console.WriteLine("Incoming payload:");
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(model));

            _context.Users.Update(user);
            _context.CleanerDetails.Update(cleaner);
            await _context.SaveChangesAsync();

            return Ok("Cleaner updated successfully.");
        }

        [HttpPut("reset-password/{driverId}")]
        public async Task<IActionResult> ResetCleanerPassword(int cleanerId, [FromBody] string newPassword)
        {
            var cleaner = await _context.CleanerDetails.FindAsync(cleanerId);
            if (cleaner == null)
                return NotFound("Cleaner not found.");

            var user = await _context.Users.FindAsync(cleaner.UserId);
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
