using TaskJect.Web.Resources;
using TaskJect.Web.Services;
using Domain.Database;
using Domain.DomainEvents;
using Domain.Handlers;
using Microsoft.Extensions.Localization;
using System.Globalization;
using Task = System.Threading.Tasks.Task;

namespace TaskJect.Web.DomainEvent
{
	public class SubscriptionCongratulationsDomainEventHandler : IDomainEventHandler<SubscriptionCongratulationsDomainEvent>
	{
		private readonly BaseNotificationQueue _queue;
		private readonly IApplicationUserRepository _users;
		private readonly ITariffPlanRepository _tariffPlanRepository;
		private readonly IEmailService _emailService;
		private readonly IStringLocalizer<SharedResources> _localizer;

		public SubscriptionCongratulationsDomainEventHandler(
			BaseNotificationQueue queue,
			IApplicationUserRepository users,
			ITariffPlanRepository tariffPlanRepository,
			IEmailService emailService,
			IStringLocalizer<SharedResources> localizer)
		{
			_queue = queue;
			_users = users;
			_tariffPlanRepository = tariffPlanRepository;
			_emailService = emailService;
			_localizer = localizer;
		}

		public bool CanHandle(IDomainEvent domainEvent) => domainEvent is SubscriptionCongratulationsDomainEvent;

		public async Task HandleAsync(IDomainEvent domainEvent)
		{
			if (domainEvent is not SubscriptionCongratulationsDomainEvent subscriptionEvent)
			{
				return;
			}

			var teamLead = await _users.GetTeamLead(subscriptionEvent.OrganizationCode);
			if (teamLead == null)
			{
				return;
			}

			var tariffPlan = await _tariffPlanRepository.Retrieve(subscriptionEvent.PlanCode);
			if (tariffPlan == null)
			{
				return;
			}

			var culture = teamLead?.Culture ?? "en";
			CultureInfo.CurrentCulture = new CultureInfo(culture);
			CultureInfo.CurrentUICulture = new CultureInfo(culture);

			var endDateCulture = subscriptionEvent.EndDate.Date.ToString("dd MMMM yyyy", CultureInfo.CurrentCulture);

			var notification = new SystemNotification
			{
				UserId = Guid.Parse(teamLead.Id),
				Title = _localizer["SubscriptionCongratulationsTitle"],
				Message = string.Format(_localizer["SubscriptionCongratulationsMessage"], 
					tariffPlan.Name, @$"<strong>{endDateCulture}</strong>")
			};

			_queue.Enqueue(notification);

			if (!string.IsNullOrEmpty(subscriptionEvent.OrganizationCode))
			{
				var emailParams = new SubscriptionEmailParams
				{
					OrganizationCode = subscriptionEvent.OrganizationCode,
					Email = teamLead.Email,
					PlanName = tariffPlan.Name,
					EndDateCultureFormat = endDateCulture,
					Type = SubscriptionEmailType.Congratulations,
				};

				await _emailService.SendEmailAsync(emailParams);
			}
		}
	}
}
