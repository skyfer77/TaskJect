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
	public class SubscriptionPaymentFailedDomainEventHandler : IDomainEventHandler<SubscriptionPaymentFailedDomainEvent>
	{
		private readonly BaseNotificationQueue _queue;
		private readonly IApplicationUserRepository _users;
		private readonly IEmailService _emailService;
		private readonly IStringLocalizer<SharedResources> _localizer;

		public SubscriptionPaymentFailedDomainEventHandler(
			BaseNotificationQueue queue,
			IApplicationUserRepository users,
			IEmailService emailService,
			IStringLocalizer<SharedResources> localizer)
		{
			_queue = queue;
			_users = users;
			_emailService = emailService;
			_localizer = localizer;
		}

		public bool CanHandle(IDomainEvent domainEvent) => domainEvent is SubscriptionPaymentFailedDomainEvent;

		public async Task HandleAsync(IDomainEvent domainEvent)
		{
			if (domainEvent is not SubscriptionPaymentFailedDomainEvent subscriptionEvent)
			{
				return;
			}

			var teamLead = await _users.GetTeamLead(subscriptionEvent.OrganizationCode);
			if (teamLead == null)
			{
				return;
			}

			var culture = teamLead?.Culture ?? "en";
			CultureInfo.CurrentCulture = new CultureInfo(culture);
			CultureInfo.CurrentUICulture = new CultureInfo(culture);

			var notification = new SystemNotification
			{
				UserId = Guid.Parse(teamLead.Id),
				Title = _localizer["SubscriptionPaymentFailedTitle"],
				Message = _localizer["SubscriptionPaymentFailedMessage"]
			};

			_queue.Enqueue(notification);

			if (!string.IsNullOrEmpty(subscriptionEvent.OrganizationCode))
			{
				var emailParams = new SubscriptionEmailParams
				{
					OrganizationCode = subscriptionEvent.OrganizationCode,
					Email = teamLead.Email,
					Type = SubscriptionEmailType.PaymentFailed,
				};

				await _emailService.SendEmailAsync(emailParams);
			}
		}
	}
}
