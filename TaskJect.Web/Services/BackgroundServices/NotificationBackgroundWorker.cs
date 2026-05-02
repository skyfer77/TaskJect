using Domain.Database;
using TaskJect.Web.Models;

namespace TaskJect.Web.Services
{
	public class NotificationBackgroundWorker : BackgroundService
	{
		private readonly BaseNotificationQueue _baseNotificationQueue; 
		private readonly TelegramMessageQueue _messageQueue;
		private readonly IServiceScopeFactory _scopeFactory;
		public NotificationBackgroundWorker(BaseNotificationQueue baseNotificationQueue,
			TelegramMessageQueue messageQueue, IServiceScopeFactory scopeFactory)
		{
			_baseNotificationQueue = baseNotificationQueue;
			_messageQueue = messageQueue;
			_scopeFactory = scopeFactory;
		}

		protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				if (_baseNotificationQueue.TryDequeue(out var notification))
				{
					try
					{
						using (var scope = _scopeFactory.CreateScope())
						{
							var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
							var userService = scope.ServiceProvider.GetRequiredService<IApplicationUserRepository>();
							
							var userId = notification.UserId.ToString();

							var systemEvent = new SystemEvent(
								userId,
								notification.Title,
								notification.Message
							);
							await notificationService.SendNotification(systemEvent);

							var user = await userService.GetUserById(userId);
							if (user != null && !string.IsNullOrEmpty(user.TelegramChatId))
							{
								_messageQueue.Enqueue(notification.UserId, notification.Message);
							}
						}
					}
					catch (Exception ex)
					{
						// логувати помилку
					}

					await System.Threading.Tasks.Task.Delay(100, stoppingToken);
				}
				else
				{
					await System.Threading.Tasks.Task.Delay(500, stoppingToken);
				}
			}
		}
	}
}
