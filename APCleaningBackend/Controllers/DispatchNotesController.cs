using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Model;

namespace APCleaningBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DispatchNotesController : ControllerBase
    {
        private readonly APCleaningBackendContext _context;

        public DispatchNotesController(APCleaningBackendContext context)
        {
            _context = context;
        }

        // GET: DispatchNotes
        [HttpGet("all")]
        public async Task<IActionResult> GetAllDispatchNotes()
        {
            var notes = await (from dn in _context.DispatchNotes
                               join b in _context.Booking
                               on dn.BookingID equals b.BookingID

                               join d in _context.DriverDetails
                               on dn.DriverID equals d.DriverDetailsID

                               join du in _context.Users
                               on d.UserId equals du.Id

                               orderby dn.Timestamp descending

                               select new
                               {
                                   dn.Id,
                                   dn.BookingID,
                                   dn.DriverID,
                                   DriverName = du.FullName,
                                   DriverImageUrl = d.DriverImageUrl,
                                   dn.Note,
                                   dn.Timestamp
                               }).ToListAsync();

            return Ok(notes);
        }
    }
}
