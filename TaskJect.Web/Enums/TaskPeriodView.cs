using TaskJect.Web.Resources;
using System.ComponentModel.DataAnnotations;

namespace TaskJect.Web.Enums
{
	public enum TaskPeriodView
	{
		[Display(Name = "None", ResourceType = typeof(SharedResources))]
		None,
		[Display(Name = "Week", ResourceType = typeof(SharedResources))]
		Week,
		[Display(Name = "TwoWeeks", ResourceType = typeof(SharedResources))]
		TwoWeeks,
		[Display(Name = "Month", ResourceType = typeof(SharedResources))]
		Month,
		[Display(Name = "ThreeMonths", ResourceType = typeof(SharedResources))]
		ThreeMonths,
		[Display(Name = "SixMonths", ResourceType = typeof(SharedResources))]
		SixMonths,
		[Display(Name = "Year", ResourceType = typeof(SharedResources))]
		Year,
		[Display(Name = "All", ResourceType = typeof(SharedResources))]
		All
	}
}
