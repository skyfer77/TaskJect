using Domain.Enums;

namespace Domain.Database
{
	public class PaymentWayForPayDto
	{
		public Guid Id { get; set; }
		public string? UserId { get; set; }
		public string OrganizationCode { get; set; }
		public string PlanCode { get; set; }
		public SubscriptionPeriodType SubscriptionPeriod { get; set; }
		public string OrderReference { get; set; } = null!;
		public decimal Amount { get; set; }
		public string Currency { get; set; }
		public string Status { get; set; }
		public string? RecToken { get; set; }
		public DateTime? DateNext { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }
	}
}
