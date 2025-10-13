using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using APCleaningBackend.Areas.Identity.Data;
using APCleaningBackend.Model;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace APCleaningBackend.Controllers
{
    [Route("api/[controller]")]
    public class ContactsController : ControllerBase
    {
        private readonly APCleaningBackendContext _context;

        public ContactsController(APCleaningBackendContext context)
        {
            _context = context;
        }

        // GET: Contacts
        public async Task<ActionResult<IEnumerable<Contact>>> GetContact()
        {
            return await _context.Contact.ToListAsync();
        }

        // POST: Contacts
        [HttpPost]
        public async Task<ActionResult<Contact>> PostContact([FromBody] Contact model)
        {
            if (string.IsNullOrWhiteSpace(model.Name) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Subject) ||
                string.IsNullOrWhiteSpace(model.Message))
            {
                return BadRequest("All fields are required.");
            }

            if (!new EmailAddressAttribute().IsValid(model.Email))
            {
                return BadRequest("Invalid email format.");
            }

            model.Name = model.Name.Trim();
            model.Email = model.Email.Trim();
            model.Subject = model.Subject.Trim();
            model.Message = model.Message.Trim();
            model.IsResolved = false;

            model.ContactID = 0;

            _context.Contact.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        [HttpPut("{id}/resolve")]
        public async Task<IActionResult> MarkResolved(int id)
        {
            var contact = await _context.Contact.FindAsync(id);
            if (contact == null) return NotFound();

            contact.IsResolved = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
