namespace DemiTicket.Api.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Token { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }

        public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
    }
}
