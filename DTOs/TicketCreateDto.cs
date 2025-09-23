using DemiTicket.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace DemiTicket.Api.DTOs
{
    public class TicketCreateDto
    {
        [Required]
        public Guid EventId { get; set; }
        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    }

}
