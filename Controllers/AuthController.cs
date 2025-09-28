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

            var verifyToken = _tokenService.GenerateSecureToken();
            var newUser = new User {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                EmailVerificationToken = verifyToken,
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var encodedToken = Uri.EscapeDataString(verifyToken);
            var verificationLink = $"https://localhost/verify-email?token={encodedToken}";
            await _emailService.SendEmailAsync(newUser.Email, "Verify your email", $"Click here: <a href='{verificationLink}'>Verify</a>");

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

            Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions {
                HttpOnly = true,
                Secure = true, // required for SameSite=None
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7),
            });
            Console.WriteLine($"Set refresh_token cookie for user {user.Email} (login)");

            foreach (var cookie in Request.Cookies) {
                Console.WriteLine($"{cookie.Key}: {cookie.Value}");
            }

            return Ok(new {
                accessToken,
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
        public async Task<IActionResult> RefreshToken()
        {
            foreach (var cookie in Request.Cookies) {
                Console.WriteLine($"{cookie.Key}: {cookie.Value}");
            }

            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("Missing refresh token");

            var tokenEntity = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (tokenEntity == null || !tokenEntity.IsActive)
                return Unauthorized("Invalid or expired refresh token");

            var user = await _context.Users.FindAsync(tokenEntity.UserId);
            if (user == null)
                return Unauthorized("User not found");

            var newAccessToken = _tokenService.CreateToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            tokenEntity.RevokedAt = DateTime.UtcNow;
            _context.RefreshTokens.Add(new RefreshToken {
                Token = newRefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            await _context.SaveChangesAsync();

            Response.Cookies.Append("refresh_token", newRefreshToken, new CookieOptions {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7),
            });
            Console.WriteLine($"Set refresh_token cookie for user {user.Email} (refresh)");

            return Ok(new { accessToken = newAccessToken, role = user.Role });
        }
    }
}