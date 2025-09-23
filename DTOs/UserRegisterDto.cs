using System.ComponentModel.DataAnnotations;

namespace DemiTicket.Api.DTOs
{
    public class UserRegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Password { get; set; } = string.Empty;
    }
}
