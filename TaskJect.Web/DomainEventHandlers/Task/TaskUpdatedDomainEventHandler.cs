using TaskJect.Web.Enums;
using TaskJect.Web.Resources;
using TaskJect.Web.Services;
using Domain.Database;
using Domain.DomainEvents;
using Domain.Handlers;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace TaskJect.Web.DomainEvent
{
    public class TaskUpdatedDomainEventHandler : IDomainEventHandler<TaskUpdatedDomainEvent>
    {
		private readonly BaseNotificationQueue _baseNotificationQueue;
		private readonly ITaskRepository _taskRepository;
		private readonly IApplicationUserRepository _applicationUserRepository;
		private readonly IStringLocalizer<SharedResources> _localizer;

		public TaskUpdatedDomainEventHandler(BaseNotificationQueue baseNotificationQueue,
			ITaskRepository taskRepository, IApplicationUserRepository applicationUserRepository,
			IStringLocalizer<SharedResources> localizer)
		{
			_baseNotificationQueue = baseNotificationQueue;
			_taskRepository = taskRepository;
			_applicationUserRepository = applicationUserRepository;
			_localizer = localizer;
		}

		public bool CanHandle(IDomainEvent domainEvent) => domainEvent is TaskUpdatedDomainEvent;

		public async System.Threading.Tasks.Task HandleAsync(IDomainEvent domainEvent)
		{
			var updateEvent = (TaskUpdatedDomainEvent)domainEvent;
			if (updateEvent == null)
			{
				return;
			}

			var task = await _taskRepository.Retrieve(updateEvent.TaskId);
			if (task == null)
			{
				return;
			}

			if (Guid.TryParse(task.AssigneeID, out var assigneeId))
			{
				var user = await _applicationUserRepository.GetUserById(task.AssigneeID);

				var culture = user?.Culture ?? "en";
				CultureInfo.CurrentCulture = new CultureInfo(culture);
				CultureInfo.CurrentUICulture = new CultureInfo(culture);

				var attr = NotificationType.TaskUpdated.GetDisplayAttribute();
				var notification = new SystemNotification()
				{
					UserId = assigneeId,
					Title = _localizer[attr.TitleKey],
					Message = string.Format(_localizer[attr.MessageKey], task.Title)
				};

				_baseNotificationQueue.Enqueue(notification);
			}
		}
	}
}
