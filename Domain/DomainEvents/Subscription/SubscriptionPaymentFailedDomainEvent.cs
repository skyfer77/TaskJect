namespace Domain.DomainEvents
{
	public class SubscriptionPaymentFailedDomainEvent : IDomainEvent
	{
		public string OrganizationCode { get; }

		public SubscriptionPaymentFailedDomainEvent(string organizationCode)
		{
			OrganizationCode = organizationCode;
		}
	}
}
