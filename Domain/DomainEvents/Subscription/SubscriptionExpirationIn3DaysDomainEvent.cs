namespace Domain.DomainEvents
{
	public class SubscriptionExpirationIn3DaysDomainEvent : IDomainEvent
	{
		public string OrganizationCode { get; }

		public SubscriptionExpirationIn3DaysDomainEvent(string organizationCode)
		{
			OrganizationCode = organizationCode;
		}
	}
}
