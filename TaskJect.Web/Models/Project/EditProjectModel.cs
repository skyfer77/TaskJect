using Domain.Database;
namespace TaskJect.Web.Models
{ 
    public class EditProjectModel
    {
        public ProjectDto Project { get; set; }
        public Dictionary<TeamDto, List<ApplicationUserLiteDto>> TeamsWithMembers { get; set; }
        public List<ProjectPermissionForUser> PermissionsUsers { get; set; }
		public List<LightOrganizationFiles>? OrganizationFiles { get; set; }
	}
}
