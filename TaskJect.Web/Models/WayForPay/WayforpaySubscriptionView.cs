using TaskJect.Web.Enums;

namespace TaskJect.Web.Models
{
	public class WayforpaySubscriptionView
	{
		public string OrganizationCode { get; set; }
		public string UserId { get; set; }
		public string PlanCode { get; set; }
		public string PlanName { get; set; }
		public SubscriptionPeriodTypeView PeriodType { get; set; }
	}
}
