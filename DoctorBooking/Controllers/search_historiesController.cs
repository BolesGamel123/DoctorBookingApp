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
    public class search_historiesController : ControllerBase
    {
        private readonly AppDBContext _context;

        public search_historiesController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<search_history>>> GetAll() => await _context.search_histories.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<search_history>> Get(long id)
        {
            var entity = await _context.search_histories.FindAsync(id);
            return entity == null ? NotFound() : entity;
        }

        [HttpPost]
        public async Task<ActionResult<search_history>> Post(search_history entity)
        {
            _context.search_histories.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = entity.id }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, search_history entity)
        {
            if (id != entity.id) return BadRequest();
            _context.Entry(entity).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { if (!_context.search_histories.Any(e => e.id == id)) return NotFound(); else throw; }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _context.search_histories.FindAsync(id);
            if (entity == null) return NotFound();
            _context.search_histories.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
