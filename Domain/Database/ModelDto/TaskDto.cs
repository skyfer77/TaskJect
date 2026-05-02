namespace Domain.Database
{
    public class TaskDto
    {
        public Guid? ID { get; set; }
        public Guid ProjectID { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? PerformanceNote { get; set; }
        public string? AssigneeID { get; set; }
        public string? CreatedByUserId { get; set; }
        public ApplicationUserLiteDto? User { get; set; }
        public Enums.TaskStatus Status { get; set; }
        public bool IsAgreedOverdue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? ComplitedDate { get; set; }
        public DateTime? ReviewDate { get; set; }
		public DateTime? ArchivedDate { get; set; }
		public Enums.Priority Priority { get; set; }
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

        public TaskDto()
        {
            
        }

        public TaskDto(Database.Task task)
        {
            ID = task.Id;
            ProjectID = task.ProjectId;
            Title = task.Title;
            //Description = task.Description;
            //PerformanceNote = task.PerformanceNote;
            AssigneeID = task.AssigneeId;
            Status = task.Status;
            IsAgreedOverdue = task.IsAgreedOverdue.HasValue? task.IsAgreedOverdue.Value : false;
            StartDate = task.StartDate;
            EndDate = task.EndDate;
            ComplitedDate = task.ComplitedDate;
            ReviewDate = task.ReviewDate;
            Priority = task.Priority;
            Complexity = task.Complexity;
            ActualHours = task.ActualHours;
            ActualMinutes = task.ActualMinutes;
            OrganizationCode = task.OrganizationCode;
            DateAdd = task.DateAdd;
            DateEdit = task.DateEdit;

        }
    }
}
