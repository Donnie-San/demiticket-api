using DemiTicket.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace DemiTicket.Api.DTOs
{
    public class UserUpdateDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Role { get; set; }
        public bool IsEmailVerified { get; set; }
    }
}
