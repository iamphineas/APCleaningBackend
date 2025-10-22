using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APCleaningBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoyaltyController : ControllerBase
    {
        private readonly APCleaningBackendContext _context;

        public LoyaltyController(APCleaningBackendContext context)
        {
            _context = context;
        }

        [HttpPost("earn")]
        public async Task<IActionResult> EarnPoints([FromBody] LoyaltyPoints points)
        {
            var existing = await _context.LoyaltyPoints
                .FirstOrDefaultAsync(lp => lp.CustomerID == points.CustomerID);

            if (existing == null)
            {
                _context.LoyaltyPoints.Add(points);
            }
            else
            {
                existing.PointsEarned += points.PointsEarned;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Points earned successfully." });
        }

        [HttpPost("redeem")]
        public async Task<IActionResult> RedeemPoints([FromBody] LoyaltyPoints points)
        {
            var existing = await _context.LoyaltyPoints
                .FirstOrDefaultAsync(lp => lp.CustomerID == points.CustomerID);

            if (existing == null || existing.PointsBalance < points.PointsRedeemed)
                return BadRequest("Insufficient points.");

            existing.PointsRedeemed += points.PointsRedeemed;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Points redeemed successfully." });
        }

        [HttpGet("balance/{customerId}")]
        public async Task<IActionResult> GetBalance(int customerId)
        {
            var points = await _context.LoyaltyPoints
                .FirstOrDefaultAsync(lp => lp.CustomerID == customerId);

            return Ok(new { balance = points?.PointsBalance ?? 0 });
        }
    }
}
