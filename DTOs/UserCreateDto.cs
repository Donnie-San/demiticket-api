using DemiTicket.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace DemiTicket.Api.DTOs
{
    public class UserCreateDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; }
    }
}
