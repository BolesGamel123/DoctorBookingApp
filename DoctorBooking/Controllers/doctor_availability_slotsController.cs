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
    public class doctor_availability_slotsController : ControllerBase
    {
        private readonly AppDBContext _context;

        public doctor_availability_slotsController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<doctor_availability_slot>>> GetAll() => await _context.doctor_availability_slots.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<doctor_availability_slot>> Get(long id)
        {
            var entity = await _context.doctor_availability_slots.FindAsync(id);
            return entity == null ? NotFound() : entity;
        }

        [HttpPost]
        public async Task<ActionResult<doctor_availability_slot>> Post(doctor_availability_slot entity)
        {
            _context.doctor_availability_slots.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = entity.id }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, doctor_availability_slot entity)
        {
            if (id != entity.id) return BadRequest();
            _context.Entry(entity).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { if (!_context.doctor_availability_slots.Any(e => e.id == id)) return NotFound(); else throw; }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var entity = await _context.doctor_availability_slots.FindAsync(id);
            if (entity == null) return NotFound();
            _context.doctor_availability_slots.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
