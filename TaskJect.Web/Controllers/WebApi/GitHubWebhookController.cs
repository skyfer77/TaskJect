using TaskJect.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace TaskJect.Web.Controllers
{
    [ApiController]
    [Route("api/github")]
    public class GitHubWebhookController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IGitHubWebhookService _gitHubWebhookServices;

        public GitHubWebhookController(IConfiguration config, IGitHubWebhookService gitHubWebhookServices)
        {
            _config = config;
            _gitHubWebhookServices = gitHubWebhookServices;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();

            var signature = Request.Headers["X-Hub-Signature-256"].ToString();
            var secret = _config["GitHub:WebhookSecret"];

            if (!verifySignature(payload, signature, secret))
            {
                return Unauthorized();
            }

            var eventType = Request.Headers["X-GitHub-Event"].ToString();

            await _gitHubWebhookServices.HandleEvent(eventType, payload);

            return Ok();
        }

        private bool verifySignature(string payload, string signature, string secret)
        {
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var hashString = "sha256=" + BitConverter.ToString(hash).Replace("-", "").ToLower();
            return hashString == signature;
        }
    }
}