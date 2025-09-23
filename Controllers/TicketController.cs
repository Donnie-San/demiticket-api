using DemiTicket.Api.Data;
using DemiTicket.Api.DTOs;
using DemiTicket.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DemiTicket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketReadDto>>> GetMyTickets()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var tickets = await _context.Tickets
                .Include(t => t.Event)
                .Where(t => t.UserId == userId)
                .ToListAsync();

            var result = tickets.Select(t => new TicketReadDto {
                Id = t.Id,
                UserId = t.UserId,
                EventId = t.EventId,
                EventTitle = t.Event.Title,
                Price = t.Event.Price,
                PaymentStatus = t.PaymentStatus,
                CreatedAt = t.CreatedAt
            });

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Create(TicketCreateDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var ev = await _context.Events.FindAsync(dto.EventId);
            if (ev == null || !ev.IsPublished) return BadRequest("Invalid event");

            var ticket = new Ticket {
                UserId = userId,
                EventId = dto.EventId,
                PaymentStatus = dto.PaymentStatus,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return Ok("Ticket created");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<ActionResult<IEnumerable<TicketReadDto>>> GetAll()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Event)
                .Include(t => t.User)
                .ToListAsync();

            var result = tickets.Select(t => new TicketReadDto {
                Id = t.Id,
                UserId = t.UserId,
                EventId = t.EventId,
                EventTitle = t.Event.Title,
                Price = t.Event.Price,
                PaymentStatus = t.PaymentStatus,
                CreatedAt = t.CreatedAt
            });

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, TicketUpdateDto dto)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            ticket.PaymentStatus = dto.PaymentStatus;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("statuses")]
        public ActionResult<IEnumerable<string>> GetPaymentStatuses()
        {
            var statuses = Enum.GetNames(typeof(PaymentStatus));
            return Ok(statuses);
        }
    }

}
