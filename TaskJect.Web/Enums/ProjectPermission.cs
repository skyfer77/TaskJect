using TaskJect.Web.Resources;
using System.ComponentModel.DataAnnotations;

namespace TaskJect.Web.Enums
{
    public enum ProjectPermission
    {
        [Display(Name = "AllUsers", ResourceType = typeof(SharedResources))]
        All,
        [Display(Name = "ProjectManager", ResourceType = typeof(SharedResources))]
        Manager,
    }
}
