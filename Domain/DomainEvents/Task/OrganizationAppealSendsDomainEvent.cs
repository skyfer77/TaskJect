namespace Domain.DomainEvents
{
    public class OrganizationAppealSendsDomainEvent : IDomainEvent
    {
        public Guid AppealId { get; }
        public Guid OrganizationId { get; }

        public OrganizationAppealSendsDomainEvent(Guid appealId, Guid organizationId)
        {
            AppealId = appealId;
            OrganizationId = organizationId;
        }
    }
}
