namespace Domain.DomainEvents
{
	public class SubscriptionExpiredDomainEvent : IDomainEvent
	{
		public string OrganizationCode { get; }

		public SubscriptionExpiredDomainEvent(string organizationCode)
		{
			OrganizationCode = organizationCode;
		}
	}
}
