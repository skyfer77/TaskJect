using TaskJect.Web.Models;
using TaskJect.Web.Resources;
using TaskJect.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace TaskJect.Web.Controllers
{
	[Route("wayforpay")]
	public class WayforpayController : Controller
	{
		private readonly IWayforpayServices _wayforpayServices;
		private readonly IStringLocalizer<ErrorResources> _localizer;
		private readonly ILogger<WayforpayController> _logger;

		public WayforpayController(IWayforpayServices wayforpayServices, IStringLocalizer<ErrorResources> localizer, 
			ILogger<WayforpayController> logger)
		{
			_wayforpayServices = wayforpayServices;
			_localizer = localizer;
			_logger = logger;
		}

		[HttpPost("subscription")]
		public async Task<IActionResult> SubscriptionPayment([FromBody] WayforpaySubscriptionView subscription)
		{
			try
			{
				subscription.OrganizationCode = this.GetOrganizationCode();
				subscription.UserId = this.GetUserId();

				var html = await _wayforpayServices.CreateRegularPaymentAsync(subscription);

				if (html == null)
				{
					return Json(new ServerResponse(false) { Message = _localizer["YourOperationWasNotSuccessful"] });
				}

				return Content(html, "text/html");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Execution error SubscriptionPayment: {Message}", ex.Message);

				return Json(new ServerResponse(false)
				{
					Message = _localizer["SERVER_ERROR"]
				});
			}
		}

		[HttpPost("change-subscription")]
		public async Task<IActionResult> ChangeSubscriptionPayment([FromBody] WayforpaySubscriptionView subscription)
		{
			try
			{
				subscription.OrganizationCode = this.GetOrganizationCode();
				subscription.UserId = this.GetUserId();

				var html = await _wayforpayServices.ChangePlanAsync(subscription);

				if (html == null)
				{
					return Json(new ServerResponse(false) { Message = _localizer["YourOperationWasNotSuccessful"] });
				}

				return Content(html, "text/html");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Execution error SubscriptionPayment: {Message}", ex.Message);

				return Json(new ServerResponse(false)
				{
					Message = _localizer["SERVER_ERROR"]
				});
			}
		}

		[HttpGet("unsubscription")]
		public async Task<IActionResult> UnsubscriptionPayment()
		{
			try
			{
				var organizationCode = this.GetOrganizationCode();

				var result = await _wayforpayServices.CancelSubscriptionAsync(organizationCode);

				if (result)
				{
					return Json(new ServerResponse(result) { Message = _localizer["YourOperationSuccessful"] });
				}

				return Json(new ServerResponse(result) { Message = _localizer["YourOperationWasNotSuccessful"] });
			}
			catch (Exception ex)
			{
				return Json(new ServerResponse(false)
				{
					Message = _localizer["SERVER_ERROR"]
				});
			}
		}
	}
}
