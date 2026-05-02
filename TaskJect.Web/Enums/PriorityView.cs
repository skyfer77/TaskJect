using TaskJect.Web.Resources;
using System.ComponentModel.DataAnnotations;

namespace TaskJect.Web.Enums
{
    public enum PriorityView
    {
        [Display(Name = "Low", ResourceType = typeof(SharedResources))]
        Low,
        [Display(Name = "Medium", ResourceType = typeof(SharedResources))]
        Medium,
        [Display(Name = "High", ResourceType = typeof(SharedResources))]
        High
    }
}
