using Domain.Database;

namespace TaskJect.Web.Models
{
    public class TaskView
    {
        public Guid? ID { get; set; }
        public Guid ProjectID { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? PerformanceNote { get; set; }
        public string? AssigneeID { get; set; }
        public string? CreatedByUserId { get; set; }
        public ApplicationUserLiteDto? User { get; set; }
        public Enums.TaskStatusView Status { get; set; }
        public bool IsAgreedOverdue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? ComplitedDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public Enums.PriorityView Priority { get; set; }
        public int Complexity { get; set; }
        public int? ActualHours { get; set; }
        public int? ActualMinutes { get; set; }
        public string OrganizationCode { get; set; }
        public DateTime? DateAdd { get; set; }
        public DateTime? DateEdit { get; set; }
        // GitHub
        public bool IsGitHubIntegration { get; set; }
        public string? GitHubBranch { get; set; }
        public string? GitHubOwner { get; set; }
        public string? GitHubRepoName { get; set; }
        public int? GitHubIssueNumber { get; set; }

        //View row 
        public bool CreateNewIssue { get; set; }

    }
}
