namespace Domain.DomainEvents
{
	public class SubscriptionCongratulationsDomainEvent : IDomainEvent
	{
		public string OrganizationCode { get; }
		public string PlanCode { get; }
		public DateTime EndDate { get; }

		public SubscriptionCongratulationsDomainEvent(string organizationCode, string planCode, DateTime endDate)
		{
			OrganizationCode = organizationCode;
			PlanCode = planCode;
			EndDate = endDate;
		}
	}
}
