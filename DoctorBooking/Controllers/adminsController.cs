using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain.Models;

namespace DoctorBooking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class adminsController : ControllerBase
    {
        private readonly AppDBContext _context;

        public adminsController(AppDBContext context)
        {
            _context = context;
        }

        // GET: api/admins
        [HttpGet]
        public async Task<ActionResult<IEnumerable<admin>>> Getadmins()
        {
            return await _context.admins.ToListAsync();
        }

        // GET: api/admins/5
        [HttpGet("{id}")]
        public async Task<ActionResult<admin>> Getadmin(long id)
        {
            var admin = await _context.admins.FindAsync(id);

            if (admin == null)
            {
                return NotFound();
            }

            return admin;
        }

        // PUT: api/admins/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Putadmin(long id, admin admin)
        {
            if (id != admin.id)
            {
                return BadRequest();
            }

            _context.Entry(admin).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!adminExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/admins
        [HttpPost]
        public async Task<ActionResult<admin>> Postadmin(admin admin)
        {
            _context.admins.Add(admin);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Getadmin", new { id = admin.id }, admin);
        }

        // DELETE: api/admins/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deleteadmin(long id)
        {
            var admin = await _context.admins.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }

            _context.admins.Remove(admin);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool adminExists(long id)
        {
            return _context.admins.Any(e => e.id == id);
        }
    }
}
