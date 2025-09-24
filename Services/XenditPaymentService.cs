using DemiTicket.Api.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace DemiTicket.Api.Services
{
    public class XenditPaymentService : IXenditPaymentService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;

        public XenditPaymentService(IConfiguration config, HttpClient http)
        {
            _config = config;
            _http = http;
        }

        public async Task<string> CreateInvoice(Guid userId, Event ev)
        {
            var payload = new {
                external_id = $"invoice-{Guid.NewGuid()}",
                payer_email = "user@example.com", // optional
                description = ev.Title,
                amount = (int)(ev.Price * 1000), // IDR
                success_redirect_url = _config["Xendit:SuccessUrl"],
                failure_redirect_url = _config["Xendit:CancelUrl"]
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.xendit.co/v2/invoices") {
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(_config["Xendit:SecretKey"])));

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(json);
            return result.invoice_url;
        }
    }
}
