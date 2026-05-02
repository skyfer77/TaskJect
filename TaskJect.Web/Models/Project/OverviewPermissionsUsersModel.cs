using Domain.Database;
namespace TaskJect.Web.Models
{
    public class OverviewPermissionsUsersModel
    {
        public ProjectDto Project { get; set; }
        public List<ProjectPermissionForUser> PermissionsUsers { get; set; }
    }
}
