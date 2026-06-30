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
    public class notificationsController : ControllerBase
    {
        private readonly AppDBContext _context;

        public notificationsController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<notification>>> GetAll() => await _context.notifications.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<notification>> Get(long id)
        {
            var entity = await _context.notifications.FindAsync(id);
            return entity == null ? NotFound() : entity;
        }

        [HttpPost]
        public async Task<ActionResult<notification>> Post(notification entity)
        {
            _context.notifications.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = entity.id }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, notification entity)
        {
            if (id != entity.id) return BadRequest();
            _context.Entry(entity).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { if (!_context.notifications.Any(e => e.id == id)) return NotFound(); else throw; }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _context.notifications.FindAsync(id);
            if (entity == null) return NotFound();
            _context.notifications.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
