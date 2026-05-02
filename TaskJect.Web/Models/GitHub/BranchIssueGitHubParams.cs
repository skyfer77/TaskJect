using TaskJect.Web.Enums;

namespace TaskJect.Web.Models
{
	public class BranchIssueGitHubParams
	{
		public Guid OrganizationId { get; set; }
		public Guid ProjectId { get; set; }
		public Guid TaskId { get; set; }
		public TaskStatusView TaskStatus { get; set; }
		public string NewBranchName { get; set; }
		public int? GitHubIssueNumber { get; set; }
		public bool CreateNewIssue { get; set; }
		public string TitleTask { get; set; }
		public string DescriptionTask { get; set; }
	}
}
