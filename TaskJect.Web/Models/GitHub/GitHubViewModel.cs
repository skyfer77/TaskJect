using TaskJect.Web.Enums;

namespace TaskJect.Web.Models
{
    public class GitHubViewModel
    {
        public bool IsGitHubIntegration { get; set; }
        public int? Issue { get; set; }
        public string? Branch { get; set; }
        public bool CreateNewIssue { get; set; }
        public bool IsCreateMode { get; set; }
        public TaskStatusView TaskStatus { get; set; }
    }
}
