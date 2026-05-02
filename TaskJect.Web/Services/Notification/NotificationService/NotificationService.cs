using Domain.Database;
using TaskJect.Web.Enums;
using TaskJect.Web.Hubs;
using TaskJect.Web.Models;
using TaskJect.Web.Resources;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;

namespace TaskJect.Web.Services
{
    public class NotificationService : INotificationService
	{
		private readonly IHubContext<NotificationHub> _hubContext;
		private readonly INotificationRepository _notificationRepository;
		private readonly IApplicationUserRepository _applicationUserRepository;
		private readonly IStringLocalizer<SharedResources> _localizer;

		public NotificationService(IHubContext<NotificationHub> hubContext, INotificationRepository notificationRepository,
			IApplicationUserRepository applicationUserRepository, IStringLocalizer<SharedResources> localizer)
		{
			_hubContext = hubContext;
			_notificationRepository = notificationRepository;
			_applicationUserRepository = applicationUserRepository;
			_localizer = localizer;
		}

		public async System.Threading.Tasks.Task SendNotification(SystemEvent systemEvent)
		{
			var title = !string.IsNullOrEmpty(systemEvent.Title) ?
				systemEvent.Title :getLocalizedTitle(systemEvent.EventType);

			var description = !string.IsNullOrEmpty(systemEvent.Message) ? 
				systemEvent.Message : getLocalizedDescription(systemEvent.EventType);

			var participantIds = (systemEvent.ParticipantIds == null || !systemEvent.ParticipantIds.Any())
				? await _applicationUserRepository.GetAllUserId()
				: systemEvent.ParticipantIds;

			var notifications = participantIds
				.Select(id => createNotification(id, title, description))
				.ToList();

			foreach (var notification in notifications)
			{
				await sendToUserAsync(notification);
			}

			await saveNotification(notifications);
		}

		private NotificationDto createNotification(string userId, string title, string description)
		{
			return new NotificationDto
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				Title = title,
				Description = description,
				IsReviewed = false,
				Created = DateTime.UtcNow,
			};
		}

		private string getLocalizedTitle(NotificationType notificationType)
		{
			var attr = notificationType.GetDisplayAttribute();
			return _localizer[attr.TitleKey];
		}

		private string getLocalizedDescription(NotificationType notificationType)
		{
			var attr = notificationType.GetDisplayAttribute();
			return _localizer[attr.MessageKey];
		}

		private async System.Threading.Tasks.Task saveNotification(List<NotificationDto> notifications)
		{
			if (notifications.Any())
			{
				await _notificationRepository.Insert(notifications);
			}
		}

		// Персонально користувачу
		private async System.Threading.Tasks.Task sendToUserAsync(NotificationDto notification)
		{
			var html = buildHtml(notification);
			await _hubContext.Clients.User(notification.UserId).SendAsync("ReceiveNotification", html);
		}


		private string buildHtml(NotificationDto notification)
		{
			var dateFormat = notification.Created.ToUniversalTime().ToString("o");
			return $@"
				<li class=""dropdown-item not-reviewed"" onmouseenter=""reviewed(this,'{notification.Id}')"">
					<div class=""d-flex align-items-start"">
						<div class=""pe-2"">
							<span class=""avatar avatar-md bg-primary-transparent avatar-rounded""><i class=""bi bi-clock-history fs-18""></i></span>
						</div>
						<div class=""flex-grow-1 d-flex align-items-center justify-content-between"">
							<div>
								<p class=""mb-0 fw-semibold"">{notification.Title}</p>
								<span class=""text-muted fw-normal fs-12 header-notification-text"">{notification.Description}</span>
								<p class=""time-ago text-muted fw-normal fs-12 mt-2 mb-0"" data-created=""{dateFormat}""></p>
							</div>
							<div>
								<button class=""min-w-fit-content text-muted mx-1 item-notification-close btn-nostyle"" onclick=""deleteNotification(this,'{notification.Id}')"">
									<i class=""ti ti-x fs-16""></i>
								</button>
							</div>
						</div>
					</div>
				</li>";
		}
	}
}
