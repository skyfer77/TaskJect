using TaskJect.Web.Enums;
using TaskJect.Web.Resources;
using TaskJect.Web.Services;
using Domain.Database;
using Domain.DomainEvents;
using Domain.Handlers;
using Google.Apis.Gmail.v1.Data;
using Microsoft.Extensions.Localization;
using System.Globalization;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace TaskJect.Web.DomainEvent
{
    public class OrganizationAppealSendsDomainEventHandler : IDomainEventHandler<OrganizationAppealSendsDomainEvent>
    {
        private readonly BaseNotificationQueue _baseNotificationQueue;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IOrganizationAppealRepository _organizationAppealRepository;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public OrganizationAppealSendsDomainEventHandler(BaseNotificationQueue baseNotificationQueue,
            IOrganizationRepository organizationRepository, IOrganizationAppealRepository organizationAppealRepository, IApplicationUserRepository applicationUserRepository,
            IStringLocalizer<SharedResources> localizer)
        {
            _baseNotificationQueue = baseNotificationQueue;
            _organizationRepository = organizationRepository;
            _organizationAppealRepository = organizationAppealRepository;
            _applicationUserRepository = applicationUserRepository;
            _localizer = localizer;
        }

        public bool CanHandle(IDomainEvent domainEvent) => domainEvent is OrganizationAppealSendsDomainEvent;

        public async System.Threading.Tasks.Task HandleAsync(IDomainEvent domainEvent)
        {
            var sendEvent = (OrganizationAppealSendsDomainEvent)domainEvent;

            var organization = await _organizationRepository.GetOrganizationById(sendEvent.OrganizationId);
            if (organization == null)
            {
                return;
            }
            var organizationAppeal = await _organizationAppealRepository.Retrieve(sendEvent.AppealId);
            if (organizationAppeal == null)
            {
                return;
            }
            var organizationTitle = organization.Name;
            var organizationAppealTitle = organizationAppeal.Title;
            var attr = NotificationType.OrganizationAppealSent.GetDisplayAttribute();
            var users = await _applicationUserRepository.GetAllAdmins();
            foreach ( var user in users)
            {
                var notification = new SystemNotification()
                {
                    UserId = Guid.Parse(user.Id),
                    Title = _localizer[attr.TitleKey],
                    Message = string.Format(_localizer[attr.MessageKey] + " {0}" + ": {1}", organizationTitle, organizationAppealTitle)
                };
                _baseNotificationQueue.Enqueue(notification);
            }
        }
    }
}
