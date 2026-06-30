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
    public class review_responsesController : ControllerBase
    {
        private readonly AppDBContext _context;

        public review_responsesController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<review_response>>> GetAll() => await _context.review_responses.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<review_response>> Get(long id)
        {
            var entity = await _context.review_responses.FindAsync(id);
            return entity == null ? NotFound() : entity;
        }

        [HttpPost]
        public async Task<ActionResult<review_response>> Post(review_response entity)
        {
            _context.review_responses.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = entity.id }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, review_response entity)
        {
            if (id != entity.id) return BadRequest();
            _context.Entry(entity).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { if (!_context.review_responses.Any(e => e.id == id)) return NotFound(); else throw; }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _context.review_responses.FindAsync(id);
            if (entity == null) return NotFound();
            _context.review_responses.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
