using TaskJect.Web.Resources;
using System.ComponentModel.DataAnnotations;

namespace TaskJect.Web.Enums
{
    public enum TaskStatusView
    {
        [Display(Name = "NotStarted", ResourceType = typeof(SharedResources))]
        NotStarted,

        [Display(Name = "InProgress", ResourceType = typeof(SharedResources))]
        InProgress,

        [Display(Name = "OnReview", ResourceType = typeof(SharedResources))]
        OnReview,

        [Display(Name = "Done", ResourceType = typeof(SharedResources))]
        Done,

        [Display(Name = "Archived", ResourceType = typeof(SharedResources))]
        Archived,

        [Display(Name = "OnHold", ResourceType = typeof(SharedResources))]
        OnHold
    }
}
