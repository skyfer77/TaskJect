using TaskJect.Web.Resources;
using System.ComponentModel.DataAnnotations;

namespace TaskJect.Web.Enums
{
    public enum UserAction
    {
        [Display(Name = "CreateTask", ResourceType = typeof(SharedResources))]
        CreateTask,
        [Display(Name = "DeleteTask", ResourceType = typeof(SharedResources))]
        DeleteTask,
        [Display(Name = "SetAssignmentsTask", ResourceType = typeof(SharedResources))]
        SetAssignmentsTask,
    }
}
