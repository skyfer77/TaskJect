namespace Domain.DomainEvents
{
	public class SubscriptionExpirationInWeekDomainEvent : IDomainEvent
	{
		public string OrganizationCode { get; }

		public SubscriptionExpirationInWeekDomainEvent(string organizationCode) 
		{ 
			OrganizationCode = organizationCode;
		}
	}
}
