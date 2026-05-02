using TaskJect.Web.Resources;
using System.ComponentModel.DataAnnotations;

namespace TaskJect.Web.Enums
{
    public enum OrganizationRolesView
    {
        [Display(Name = "Member", ResourceType = typeof(SharedResources))]
        Member,
        [Display(Name = "Developer", ResourceType = typeof(SharedResources))]
        Developer = 1,
        [Display(Name = "Designer", ResourceType = typeof(SharedResources))]
        Designer = 2,
        [Display(Name = "QA", ResourceType = typeof(SharedResources))]
        QA,
        [Display(Name = "DevOps", ResourceType = typeof(SharedResources))]
        DevOps,
        [Display(Name = "SalesManager", ResourceType = typeof(SharedResources))]
        SalesManager,
        [Display(Name = "Analyst", ResourceType = typeof(SharedResources))]
        Analyst,
        [Display(Name = "HRManager", ResourceType = typeof(SharedResources))]
        HRManager,
        [Display(Name = "MarketingManager", ResourceType = typeof(SharedResources))]
        MarketingManager,
        [Display(Name = "ProjectManager", ResourceType = typeof(SharedResources))]
        ProjectManager = 50,
        [Display(Name = "TeamLead", ResourceType = typeof(SharedResources))]
        TeamLead = 99
    }
}
