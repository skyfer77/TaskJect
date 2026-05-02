namespace Domain.DomainEvents
{
	public class SubscriptionPaymentRefundedDomainEvent : IDomainEvent
	{
		public string OrganizationCode { get; }

		public SubscriptionPaymentRefundedDomainEvent(string organizationCode)
		{
			OrganizationCode = organizationCode;
		}
	}
}
