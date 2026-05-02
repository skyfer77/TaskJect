using TaskJect.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace TaskJect.Web.Controllers
{
	[ApiController]
	[Route("api/wayforpay")]
	public class WayforpayWebhookController : Controller
	{
		private readonly IWayforpayWebhookService _wayforpayWebhookService;
		private readonly ILogger<WayforpayWebhookController> _logger;

		public WayforpayWebhookController(IWayforpayWebhookService wayforpayWebhookService, 
			ILogger<WayforpayWebhookController> logger)
		{
			_wayforpayWebhookService = wayforpayWebhookService;
			_logger = logger;
		}

		[HttpPost("finishpayment")]
		public async Task<IActionResult> Webhook()
		{
			using var reader = new StreamReader(Request.Body);
			var body = await reader.ReadToEndAsync();

			if (string.IsNullOrWhiteSpace(body))
			{
				_logger.LogWarning("Webhook: Empty request body");
				return Ok(new { status = "fail", reason = "Empty request" });
			}

			try
			{
				var json = JsonSerializer.Deserialize<JsonElement>(body);

				var result = await _wayforpayWebhookService.ProcessWebhookAsync(body);

				if (!result)
				{
					return Ok(new { status = "fail" });
				}

				return Ok(new { status = "accepted" });
			}
			catch (UnauthorizedAccessException unex)
			{
				_logger.LogError(unex, $"Invalid signature. Details: {unex.Message}");
				return Unauthorized("Invalid signature. Details:" + unex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Webhook: Unexpected error");
				return Ok(new { status = "fail", reason = "Payment failed . Details: " + ex.Message });
			}
		}
	}
}
