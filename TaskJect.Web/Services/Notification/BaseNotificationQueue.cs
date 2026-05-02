using System.Collections.Concurrent;

namespace TaskJect.Web.Services
{
	public class BaseNotificationQueue
	{
		private readonly ConcurrentQueue<SystemNotification> _notifications = new();

		public void Enqueue(SystemNotification notification)
		{
			_notifications.Enqueue(notification);
		}

		public bool TryDequeue(out SystemNotification notification)
		{
			return _notifications.TryDequeue(out notification);
		}
	}

	public class SystemNotification
	{
		public Guid UserId { get; set; }
		public string Title { get; set; }
		public string Message { get; set; }
	}
}
