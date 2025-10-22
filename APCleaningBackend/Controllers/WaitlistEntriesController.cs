using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Model;
using APCleaningBackend.Services;
using Microsoft.AspNetCore.Authorization;

namespace APCleaningBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WaitlistEntriesController : ControllerBase
    {
        private readonly APCleaningBackendContext _context;
        private readonly IEmailService _emailService;


        public WaitlistEntriesController(APCleaningBackendContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }


        [HttpPost]
        public async Task<IActionResult> SubmitEmail([FromBody] WaitlistEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.Email))
                return BadRequest("Email is required.");

            var email = new WaitlistEntry
            {
                Email = entry.Email,
                SubmittedAt = DateTime.UtcNow
            };
            var exists = await _context.WaitlistEntry.AnyAsync(e => e.Email.ToLower() == entry.Email.ToLower());
            if (exists)
                return Conflict("Email already on the waitlist.");

            _context.WaitlistEntry.Add(email);
            await _context.SaveChangesAsync();
            await _emailService.SendWaitlistConfirmationAsync(entry.Email);
            return Ok(new { message = "Email saved" });
        }

        [HttpGet]
        public async Task<IActionResult> GetWaitlist()
        {
            var entries = await _context.WaitlistEntry
                .OrderByDescending(e => e.SubmittedAt)
                .ToListAsync();

            return Ok(entries);
        }
    }
}
