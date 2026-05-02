using Domain.Database;

namespace TaskJect.Web.Models
{
	public class OverviewTaskModel
	{
		public TaskView Task { get; set; }
		public List<LightOrganizationFiles>? OrganizationFiles { get; set; }
        public bool GitHubIntegration { get; set; }
    }
}
