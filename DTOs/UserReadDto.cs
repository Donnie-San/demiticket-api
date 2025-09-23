using DemiTicket.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace DemiTicket.Api.DTOs
{
    public class UserReadDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsEmailVerified { get; set; }
        public string EmailVerificationToken { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }
    }
}
