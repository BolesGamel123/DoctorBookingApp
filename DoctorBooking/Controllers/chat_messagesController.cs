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
    public class chat_messagesController : ControllerBase
    {
        private readonly AppDBContext _context;

        public chat_messagesController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<chat_message>>> GetAll() => await _context.chat_messages.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<chat_message>> Get(long id)
        {
            var entity = await _context.chat_messages.FindAsync(id);
            return entity == null ? NotFound() : entity;
        }

        [HttpPost]
        public async Task<ActionResult<chat_message>> Post(chat_message entity)
        {
            _context.chat_messages.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = entity.id }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, chat_message entity)
        {
            if (id != entity.id) return BadRequest();
            _context.Entry(entity).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { if (!_context.chat_messages.Any(e => e.id == id)) return NotFound(); else throw; }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _context.chat_messages.FindAsync(id);
            if (entity == null) return NotFound();
            _context.chat_messages.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
