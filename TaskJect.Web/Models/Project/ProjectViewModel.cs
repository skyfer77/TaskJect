using Domain.Database;
namespace TaskJect.Web.Models
{
    public class ProjectViewModel
    {
        public ProjectDto Project { get; set; }
        public List<ProjectUserPermissionDto> ProjectUserPermissions { get; set; }
        public string? RepoFullName { get; set; }
    }
}
