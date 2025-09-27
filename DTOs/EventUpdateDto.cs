using System.ComponentModel.DataAnnotations;

namespace DemiTicket.Api.DTOs
{
    public class EventUpdateDto
    {
        [Required]
        [StringLength(80)]
        public string Title { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        [Range(0, double.MaxValue)]
        public double Price { get; set; }
        [Required]
        public DateTime EventDate { get; set; }
        public bool IsPublished { get; set; }
    } 
}