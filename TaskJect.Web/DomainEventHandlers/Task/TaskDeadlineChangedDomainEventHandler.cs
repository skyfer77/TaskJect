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
    public class TaskDeadlineChangedDomainEventHandler : IDomainEventHandler<TaskDeadlineChangedDomainEvent>
    {
		private readonly BaseNotificationQueue _baseNotificationQueue;
		private readonly ITaskRepository _taskRepository;
		private readonly IApplicationUserRepository _applicationUserRepository;
		private readonly IStringLocalizer<SharedResources> _localizer;

		public TaskDeadlineChangedDomainEventHandler(BaseNotificationQueue baseNotificationQueue,
			ITaskRepository taskRepository, IApplicationUserRepository applicationUserRepository,
			IStringLocalizer<SharedResources> localizer)
		{
			_baseNotificationQueue = baseNotificationQueue;
			_taskRepository = taskRepository;
			_applicationUserRepository = applicationUserRepository;
			_localizer = localizer;
		}

		public bool CanHandle(IDomainEvent domainEvent) => domainEvent is TaskDeadlineChangedDomainEvent;

		public async System.Threading.Tasks.Task HandleAsync(IDomainEvent domainEvent)
		{
			var deadlineEvent = (TaskDeadlineChangedDomainEvent)domainEvent;
			if (deadlineEvent == null)
			{
				return;
			}

			var task = await _taskRepository.Retrieve(deadlineEvent.TaskId);
			if (task == null || string.IsNullOrEmpty(task.AssigneeID) || !Guid.TryParse(task.AssigneeID, out Guid assigneeId))
			{
				return;
			}

			var taskTitle = task.Title;

			var user = await _applicationUserRepository.GetUserById(task.AssigneeID);

			var culture = user?.Culture ?? "en";
			var cultureInfo = new CultureInfo(culture);

			CultureInfo.CurrentCulture = cultureInfo;
			CultureInfo.CurrentUICulture = cultureInfo;

			if (deadlineEvent.OldDeadline == null || deadlineEvent.OldDeadline == default)
			{
				var attr = NotificationType.TaskDedlineSet.GetDisplayAttribute();
				var notification = new SystemNotification()
				{
					UserId = assigneeId,
					Title = _localizer[attr.TitleKey],
					Message = string.Format(
						_localizer[attr.MessageKey],
								taskTitle,
						deadlineEvent.NewDeadline?.ToString("d", cultureInfo)
					)
				};

				_baseNotificationQueue.Enqueue(notification);
			}
			else if (isDifferentDate(deadlineEvent.OldDeadline, deadlineEvent.NewDeadline))
			{
				var attr = NotificationType.TaskDedlineChanged.GetDisplayAttribute();
				var notification = new SystemNotification()
				{
					UserId = assigneeId,
					Title = _localizer[attr.TitleKey],
					Message = string.Format(
						 _localizer[attr.MessageKey],
						 taskTitle,
						 deadlineEvent.OldDeadline?.ToString("d", cultureInfo),
						 deadlineEvent.NewDeadline?.ToString("d", cultureInfo)
					 )
				};

				_baseNotificationQueue.Enqueue(notification);
			}
		}

		private bool isDifferentDate(DateTime? oldDeadline, DateTime? newDeadline)
		{
			return newDeadline != null && oldDeadline == null || oldDeadline == default ||
				(oldDeadline.Value.Year != newDeadline.Value.Year || oldDeadline.Value.Month != newDeadline.Value.Month ||
				oldDeadline.Value.Day != newDeadline.Value.Day);
		}
	}
}
