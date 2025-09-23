using DemiTicket.Api.Models;

namespace DemiTicket.Api.DTOs
{
    public class TicketReadDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid EventId { get; set; }
        public string EventTitle { get; set; }
        public double Price { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
