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
    public class bookingsController : ControllerBase
    {
        private readonly AppDBContext _context;

        public bookingsController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<booking>>> GetAll() => await _context.bookings.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<booking>> Get(long id)
        {
            var entity = await _context.bookings.FindAsync(id);
            return entity == null ? NotFound() : entity;
        }

        [HttpPost]
        public async Task<ActionResult<booking>> Post(booking entity)
        {
            _context.bookings.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = entity.id }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, booking entity)
        {
            if (id != entity.id) return BadRequest();
            _context.Entry(entity).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { if (!_context.bookings.Any(e => e.id == id)) return NotFound(); else throw; }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _context.bookings.FindAsync(id);
            if (entity == null) return NotFound();
            _context.bookings.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
