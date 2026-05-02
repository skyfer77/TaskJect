using Domain.Database;
namespace TaskJect.Web.Models
{
    public class ProjectPermissionForUser
    {
        public string UserName { get; set; }
        public string UserSurname { get; set; }
        public string Role { get; set; }
        public bool IsProjectManager { get; set; }
        public ProjectUserPermissionDto ProjectUserPermission { get; set; }
    }
}
