using TaskJect.Web.Resources;
using System.ComponentModel.DataAnnotations;

namespace TaskJect.Web.Enums
{
    public enum AppealStatusView
    {
        [Display(Name = "InProcessing", ResourceType = typeof(SharedResources))]
        InProcessing,
        [Display(Name = "TakenToWork", ResourceType = typeof(SharedResources))]
        TakenToWork,
        [Display(Name = "Postponed", ResourceType = typeof(SharedResources))]
        Postponed,
        [Display(Name = "Done", ResourceType = typeof(SharedResources))]
        Done,
        [Display(Name = "Rejected", ResourceType = typeof(SharedResources))]
        Rejected
    }
}
