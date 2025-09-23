using DemiTicket.Api.Data;
using DemiTicket.Api.DTOs;
using DemiTicket.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemiTicket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventReadDto>>> GetAllPublished()
        {
            var events = await _context.Events
                .Where(e => e.IsPublished)
                .ToListAsync();

            var result = events.Select(e => new EventReadDto {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Price = e.Price,
                EventDate = e.EventDate,
                IsPublished = e.IsPublished,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            });

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<ActionResult<IEnumerable<EventReadDto>>> GetAll()
        {
            var events = await _context.Events.ToListAsync();

            var result = events.Select(e => new EventReadDto {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Price = e.Price,
                EventDate = e.EventDate,
                IsPublished = e.IsPublished,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            });

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<EventReadDto>> GetById(Guid id)
        {
            var e = await _context.Events.FindAsync(id);
            if (e == null) return NotFound();

            var dto = new EventReadDto {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Price = e.Price,
                EventDate = e.EventDate,
                IsPublished = e.IsPublished,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            };

            return Ok(dto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> Create(EventCreateDto dto)
        {
            var e = new Event {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                EventDate = dto.EventDate,
                IsPublished = dto.IsPublished,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Events.Add(e);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = e.Id }, e);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, EventUpdateDto dto)
        {
            var e = await _context.Events.FindAsync(id);
            if (e == null) return NotFound();

            e.Title = dto.Title;
            e.Description = dto.Description;
            e.Price = dto.Price;
            e.EventDate = dto.EventDate;
            e.IsPublished = dto.IsPublished;
            e.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var e = await _context.Events.FindAsync(id);
            if (e == null) return NotFound();

            _context.Events.Remove(e);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
