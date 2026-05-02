using Domain.Database;

namespace TaskJect.Web.Models
{
    public class DetailsTaskViewModel
    {
        public EditTaskModel TaskEditModel { get; set; }
        public ProjectPermissionModel Permissions { get; set; }
        public GitHubViewModel GitHubViewModel { get; set; }
    }
}