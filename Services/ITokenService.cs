using DemiTicket.Api.Models;

namespace DemiTicket.Api.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}