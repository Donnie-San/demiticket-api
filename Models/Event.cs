using System.ComponentModel.DataAnnotations;

namespace DemiTicket.Api.Models
{
    public class Event
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(80)]
        public string Title { get; set; } = string.Empty;
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        [Range(0, double.MaxValue)]
        public double Price { get; set; }
        public DateTime EventDate { get; set; }
        public bool IsPublished { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Ticket> Tickets { get; set; }
    }
}
