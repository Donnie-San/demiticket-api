using DemiTicket.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DemiTicket.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected AppDbContext()
        {
        }
    }
}
