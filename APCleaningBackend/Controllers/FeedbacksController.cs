using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Model;
using System.Security.Claims;
using APCleaningBackend.ViewModel;

namespace APCleaningBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class FeedbacksController : ControllerBase
    {
        private readonly APCleaningBackendContext _context;

        public FeedbacksController(APCleaningBackendContext context)
        {
            _context = context;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackInputModel input)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated.");

            if (input.Rating < 1 || input.Rating > 5)
                return BadRequest("Rating must be between 1 and 5.");

            var feedback = new Feedback
            {
                CustomerID = userId,
                Rating = input.Rating,
                CleanerID = input.CleanerID,
                Comments = input.Comments,
                CreatedDate = DateTime.UtcNow
            };

            _context.Feedback.Add(feedback);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Feedback submitted successfully." });
        }

        [HttpGet("cleaner")]
        public async Task<IActionResult> GetFeedbackForCleaner()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cleaner = await _context.CleanerDetails
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cleaner == null)
                return NotFound("Cleaner profile not found.");

            var feedback = await _context.Feedback
                .Where(f => f.CleanerID == cleaner.CleanerDetailsID)
                .OrderByDescending(f => f.CreatedDate)
                .ToListAsync();

            return Ok(feedback);
        }

        [HttpGet("completed-cleaners")]
        public async Task<IActionResult> GetCleanersWithCompletedBookings()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cleaners = await (
                from b in _context.Booking
                where b.CustomerID == userId &&
                      b.BookingStatus == "Completed" &&
                      b.AssignedCleanerID != null

                join cd in _context.CleanerDetails
                on b.AssignedCleanerID equals cd.CleanerDetailsID

                join cu in _context.Users
                on cd.UserId equals cu.Id

                group new { cd, cu } by cd.CleanerDetailsID into g
                select new
                {
                    CleanerDetailsID = g.Key,
                    FullName = g.Select(x => x.cu.FullName).FirstOrDefault(),
                    CleanerImageUrl = g.Select(x => x.cd.CleanerImageUrl).FirstOrDefault()
                }

            ).ToListAsync();

            return Ok(cleaners);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllFeedback()
        {
            var feedback = await (
                from f in _context.Feedback

                join cd in _context.CleanerDetails
                on f.CleanerID equals cd.CleanerDetailsID

                join cu in _context.Users
                on cd.UserId equals cu.Id

                orderby f.CreatedDate descending

                select new
                {
                    f.FeedbackID,
                    f.Rating,
                    f.Comments,
                    f.CreatedDate,
                    CleanerName = cu.FullName,
                    CleanerImageUrl = cd.CleanerImageUrl
                }
            ).ToListAsync();

            return Ok(feedback);
        }

    }
}
