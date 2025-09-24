using DemiTicket.Api.Models;

namespace DemiTicket.Api.Services
{
    public interface IStripePaymentService
    {
        Task<string> CreateCheckoutSession(Guid userId, Event ev);
    }
}