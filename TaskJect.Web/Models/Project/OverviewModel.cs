using Domain.Database;
namespace TaskJect.Web.Models
{
    public class OverviewModel
    {
        public ProjectDto Project { get; set; }
        public TeamDto Team { get; set; }
        public List<ApplicationUserLiteDto> User { get; set; }
		public List<LightOrganizationFiles>? OrganizationFiles { get; set; }
	}
}
