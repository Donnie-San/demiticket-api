using DemiTicket.Api.Data;
using DemiTicket.Api.DTOs;
using DemiTicket.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemiTicket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetAll()
        {
            var users = await _context.Users.ToListAsync();
            var result = users.Select(u => new UserReadDto {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role,
                IsEmailVerified = u.IsEmailVerified,
                EmailVerificationToken = u.EmailVerificationToken,
                CreatedAt = u.CreatedAt,
                LastLogin = u.LastLogin ?? DateTime.MinValue
            });

            return Ok(result);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserReadDto>> GetById(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var result = new UserReadDto {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                IsEmailVerified = user.IsEmailVerified,
                EmailVerificationToken = user.EmailVerificationToken,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin ?? DateTime.MinValue
            };

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> Create(UserCreateDto dto)
        {
            var user = new User {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, new User {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                IsEmailVerified = user.IsEmailVerified,
                EmailVerificationToken = user.EmailVerificationToken,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin ?? DateTime.MinValue
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Email = dto.Email;
            user.Role = dto.Role;
            user.IsEmailVerified = dto.IsEmailVerified;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
