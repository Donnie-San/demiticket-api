using System.ComponentModel.DataAnnotations;

namespace DemiTicket.Api.Models
{
    public class Ticket
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public Guid EventId { get; set; }

        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }
        public Event Event { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed
    }
}
