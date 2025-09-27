using BCrypt.Net;
using DemiTicket.Api.Data;
using DemiTicket.Api.DTOs;
using DemiTicket.Api.Models;
using DemiTicket.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemiTicket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public AuthController(AppDbContext context, ITokenService tokenService, IEmailService emailService)
        {
            _context = context;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("Email already registered!");

            var token = _tokenService.GenerateSecureToken();
            var user = new User {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                EmailVerificationToken = token,
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var verificationLink = $"https://yourfrontend.com/verify-email?token={token}"; // Not yet decided
            await _emailService.SendEmailAsync(user.Email, "Verify your email", $"Click here: <a href='{verificationLink}'>Verify</a>");

            return Ok("User registered. Please check your email!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid Credentials");

            if (!user.IsEmailVerified)
                return Unauthorized("Please verify your email before logging in.");

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var accessToken = _tokenService.CreateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var tokenEntity = new RefreshToken {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _context.RefreshTokens.Add(tokenEntity);
            await _context.SaveChangesAsync();


            return Ok(new {
                accessToken,
                refreshToken,
                Role = user.Role
            });
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
            if (user == null) return BadRequest("Invalid token");

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            await _context.SaveChangesAsync();

            return Ok("Email verified successfully");
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var tokenEntity = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken && t.IsActive);

            if (tokenEntity == null) return Unauthorized("Invalid or expired refresh token");

            var user = await _context.Users.FindAsync(tokenEntity.UserId);
            if (user == null) return Unauthorized("User not found");
            
            var newAccessToken = _tokenService.CreateToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            tokenEntity.RevokedAt = DateTime.UtcNow;
            _context.RefreshTokens.Add(new RefreshToken {
                Token = newRefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            await _context.SaveChangesAsync();
            return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
        }
    }
}
