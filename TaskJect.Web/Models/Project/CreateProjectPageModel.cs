using Domain.Database;
namespace TaskJect.Web.Models
{
    public class CreateProjectPageModel
    {
        public Dictionary<TeamDto, List<ApplicationUserLiteDto>> TeamWithUsers { get; set; }
        public List<ProjectPermissionForUser> PermissionsUsers { get; set; }

    }
}
