using DemiTicket.Api.Data;
using DemiTicket.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using System.Security.Claims;

namespace DemiTicket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckoutController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IStripePaymentService _stripePaymentService;
        private readonly IXenditPaymentService _xenditPaymentService;

        public CheckoutController(AppDbContext context, IStripePaymentService paymentService, IXenditPaymentService xenditPaymentService)
        {
            _context = context;
            _stripePaymentService = paymentService;
            _xenditPaymentService = xenditPaymentService;
        }

        [Authorize]
        [HttpPost("stripe/{eventId}")]
        public async Task<IActionResult> CreateStripeCheckoutSession(Guid eventId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId && e.IsPublished);
            if (ev == null) return BadRequest("Invalid event");

            var sessionUrl = await _stripePaymentService.CreateCheckoutSession(userId, ev);
            return Ok(new { sessionUrl });
        }

        [Authorize]
        [HttpPost("xendit/{eventId}")]
        public async Task<IActionResult> CreateXenditCheckoutSession(Guid eventId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId && e.IsPublished);
            if (ev == null) return BadRequest("Invalid event");

            var invoiceUrl = await _xenditPaymentService.CreateInvoice(userId, ev, userEmail);
            return Ok(new { invoiceUrl });
        }
    }

}
