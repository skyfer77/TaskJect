using Domain.Database;
using Domain.DomainEvents;
using Domain.Handlers;
using TaskJect.Web.Resources;
using TaskJect.Web.Services;
using Microsoft.Extensions.Localization;
using System.Globalization;
using TaskJect.Web.Enums;

namespace TaskJect.Web.DomainEvent
{
    public class TaskCreatedDomainEventHandler : IDomainEventHandler<TaskCreatedDomainEvent>
    {
		private readonly BaseNotificationQueue _baseNotificationQueue;
		private readonly ITaskRepository _taskRepository;
		private readonly IApplicationUserRepository _applicationUserRepository;
		private readonly IStringLocalizer<SharedResources> _localizer;

		public TaskCreatedDomainEventHandler(BaseNotificationQueue baseNotificationQueue,
			ITaskRepository taskRepository, IApplicationUserRepository applicationUserRepository,
			IStringLocalizer<SharedResources> localizer)
		{
			_baseNotificationQueue = baseNotificationQueue;
			_taskRepository = taskRepository;
			_applicationUserRepository = applicationUserRepository;
			_localizer = localizer;
		}

		public bool CanHandle(IDomainEvent domainEvent) => domainEvent is TaskCreatedDomainEvent;

		public async System.Threading.Tasks.Task HandleAsync(IDomainEvent domainEvent)
		{
			var taskEvent = (TaskCreatedDomainEvent)domainEvent;
			if (taskEvent == null)
			{
				return;
			}

			var task = await _taskRepository.Retrieve(taskEvent.TaskId);
			if (!string.IsNullOrEmpty(task.AssigneeID) && Guid.TryParse(task.AssigneeID, out var executorId))
			{
				var user = await _applicationUserRepository.GetUserById(task.AssigneeID);

				var culture = user?.Culture ?? "en";
				CultureInfo.CurrentCulture = new CultureInfo(culture);
				CultureInfo.CurrentUICulture = new CultureInfo(culture);

				var attr = NotificationType.TaskCreated.GetDisplayAttribute();
				var notification = new SystemNotification()
				{
					UserId = executorId,
					Title = _localizer[attr.TitleKey],
					Message = string.Format(_localizer[attr.MessageKey], task.Title)
				};

				_baseNotificationQueue.Enqueue(notification);
			}
		}
	}
}
