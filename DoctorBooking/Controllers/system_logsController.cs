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
    public class system_logsController : ControllerBase
    {
        private readonly AppDBContext _context;

        public system_logsController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<system_log>>> GetAll() => await _context.system_logs.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<system_log>> Get(long id)
        {
            var entity = await _context.system_logs.FindAsync(id);
            return entity == null ? NotFound() : entity;
        }

        [HttpPost]
        public async Task<ActionResult<system_log>> Post(system_log entity)
        {
            _context.system_logs.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = entity.id }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, system_log entity)
        {
            if (id != entity.id) return BadRequest();
            _context.Entry(entity).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { if (!_context.system_logs.Any(e => e.id == id)) return NotFound(); else throw; }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _context.system_logs.FindAsync(id);
            if (entity == null) return NotFound();
            _context.system_logs.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
