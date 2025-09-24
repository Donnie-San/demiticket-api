using DemiTicket.Api.Models;

namespace DemiTicket.Api.Services
{
    public interface IXenditPaymentService
    {
        Task<string> CreateInvoice(Guid userId, Event ev);
    }
}
