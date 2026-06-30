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
    public class favoritesController : ControllerBase
    {
        private readonly AppDBContext _context;

        public favoritesController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<favorite>>> GetAll() => await _context.favorites.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<favorite>> Get(long id)
        {
            var entity = await _context.favorites.FindAsync(id);
            return entity == null ? NotFound() : entity;
        }

        [HttpPost]
        public async Task<ActionResult<favorite>> Post(favorite entity)
        {
            _context.favorites.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = entity.id }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, favorite entity)
        {
            if (id != entity.id) return BadRequest();
            _context.Entry(entity).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { if (!_context.favorites.Any(e => e.id == id)) return NotFound(); else throw; }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _context.favorites.FindAsync(id);
            if (entity == null) return NotFound();
            _context.favorites.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
