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
    public class notification_preferencesController : ControllerBase
    {
        private readonly AppDBContext _context;

        public notification_preferencesController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<notification_preference>>> GetAll() => await _context.notification_preferences.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<notification_preference>> Get(long id)
        {
            var entity = await _context.notification_preferences.FindAsync(id);
            return entity == null ? NotFound() : entity;
        }

        [HttpPost]
        public async Task<ActionResult<notification_preference>> Post(notification_preference entity)
        {
            _context.notification_preferences.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = entity.id }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, notification_preference entity)
        {
            if (id != entity.id) return BadRequest();
            _context.Entry(entity).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { if (!_context.notification_preferences.Any(e => e.id == id)) return NotFound(); else throw; }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _context.notification_preferences.FindAsync(id);
            if (entity == null) return NotFound();
            _context.notification_preferences.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
