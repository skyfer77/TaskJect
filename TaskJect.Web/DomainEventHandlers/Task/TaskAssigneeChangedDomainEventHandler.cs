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
    public class TaskAssigneeChangedDomainEventHandler : IDomainEventHandler<TaskAssigneeChangedDomainEvent>
    {
		private readonly BaseNotificationQueue _baseNotificationQueue;
		private readonly ITaskRepository _taskRepository;
		private readonly IApplicationUserRepository _applicationUserRepository;
		private readonly IStringLocalizer<SharedResources> _localizer;

		public TaskAssigneeChangedDomainEventHandler(BaseNotificationQueue baseNotificationQueue,
			ITaskRepository taskRepository, IApplicationUserRepository applicationUserRepository,
			IStringLocalizer<SharedResources> localizer)
		{
			_baseNotificationQueue = baseNotificationQueue;
			_taskRepository = taskRepository;
			_applicationUserRepository = applicationUserRepository;
			_localizer = localizer;
		}

		public bool CanHandle(IDomainEvent domainEvent) => domainEvent is TaskAssigneeChangedDomainEvent;

		public async System.Threading.Tasks.Task HandleAsync(IDomainEvent domainEvent)
		{
			var assigneeEvent = (TaskAssigneeChangedDomainEvent)domainEvent;
			if (assigneeEvent == null)
			{
				return;
			}

			var task = await _taskRepository.Retrieve(assigneeEvent.TaskId);
			if (task == null)
			{
				return;
			}

			var taskTitle = task.Title;
			var newAssigneeId = assigneeEvent.NewAssigneeId;

			var user = await _applicationUserRepository.GetUserById(newAssigneeId.ToString());

			var culture = user?.Culture ?? "en";
			CultureInfo.CurrentCulture = new CultureInfo(culture);
			CultureInfo.CurrentUICulture = new CultureInfo(culture);

			var attr = NotificationType.TaskAssigneeChanged.GetDisplayAttribute();

			var notification = new SystemNotification()
			{
				UserId = newAssigneeId,
				Title = _localizer[attr.TitleKey],
				Message = string.Format(_localizer[attr.MessageKey], taskTitle)
			};

			_baseNotificationQueue.Enqueue(notification);
		}
	}
}
