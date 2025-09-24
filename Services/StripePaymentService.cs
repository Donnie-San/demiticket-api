using DemiTicket.Api.Models;
using Microsoft.Extensions.Configuration;
using Stripe.Checkout;

namespace DemiTicket.Api.Services
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly IConfiguration _config;

        public StripePaymentService(IConfiguration config)
        {
            _config = config;
            Stripe.StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
        }

        public async Task<string> CreateCheckoutSession(Guid userId, Event ev)
        {
            var options = new SessionCreateOptions {
                PaymentMethodTypes = ["card"],
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(ev.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = ev.Title
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = _config["Stripe:SuccessUrl"],
                CancelUrl = _config["Stripe:CancelUrl"],
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId.ToString() },
                    { "eventId", ev.Id.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return session.Url;
        }
    }
}