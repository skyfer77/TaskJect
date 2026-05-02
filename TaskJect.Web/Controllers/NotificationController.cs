using TaskJect.Web.Models;
using TaskJect.Web.Resources;
using Domain.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace TaskJect.Web.Controllers
{
	public class NotificationController : Controller
	{
		private readonly INotificationRepository _notificationRepository;
		private readonly IStringLocalizer<ErrorResources> _localizer;

		public NotificationController(INotificationRepository notificationRepository,
			IStringLocalizer<ErrorResources> localizer)
		{
			_notificationRepository = notificationRepository;
			_localizer = localizer;
		}

		[HttpGet]
		public async Task<IActionResult> GetUserNotifications()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var notifications = await _notificationRepository.Retrieve(userId);

			return PartialView("_ListNotificationPartial", notifications);
		}

		[HttpPost]
		public async Task<JsonResult> Reviewed(Guid id)
		{
			var result = await _notificationRepository.SetIsReviewed(id);
			if (!result)
			{
				return Json(new ServerResponse(result) { Message = _localizer["YourOperationWasNotSuccessful"] });
			}

			return Json(new ServerResponse(result));
		}

		[HttpGet]
		public async Task<JsonResult> ReviewedAll()
		{
			var userId = this.GetUserId();
			var result = await _notificationRepository.SetIsReviewedAll(userId);
			if (!result)
			{
				return Json(new ServerResponse(result) { Message = _localizer["YourOperationWasNotSuccessful"] });
			}

			return Json(new ServerResponse(result));
		}

		[HttpPost]
		public async Task<JsonResult> Delete(Guid id)
		{
			var result = await _notificationRepository.Delete(id);
			if (!result)
			{
				return Json(new ServerResponse(result) { Message = _localizer["YourOperationWasNotSuccessful"] });
			}

			return Json(new ServerResponse(result));
		}
	}
}
