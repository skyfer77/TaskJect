using TaskJect.Web.Resources;
using System.ComponentModel.DataAnnotations;

namespace TaskJect.Web.Enums
{
	public enum SubscriptionPeriodTypeView
	{
		[Display(Name = "Monthly", ResourceType = typeof(SharedResources))]
		Month,
		[Display(Name = "Annual", ResourceType = typeof(SharedResources))]
		Year,
	}
}
