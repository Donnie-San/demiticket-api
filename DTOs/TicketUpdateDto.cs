using DemiTicket.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace DemiTicket.Api.DTOs
{
    public class TicketUpdateDto
    {
        [Required]
        public PaymentStatus PaymentStatus { get; set; }
    }
}
