using Domain.Enums;

namespace Domain.Database
{ 
    public class ProjectDto
    {
        public Guid? ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string ManagerID { get; set; }
        public virtual ApplicationUserLiteDto Manager { get; set; }
        public string Client { get; set; }
        public Enums.TaskStatus Status { get; set; }
        public int StatusCompleted { get; set; }
        public Priority Priority { get; set; }
        public Guid? TeamId { get; set; }
        public virtual TeamDto Team { get; set; }
        public int TaskTotal { get; set; }
        public int TaskCompleted { get; set; }
        public virtual List<TaskDto> TaskProject { get; set; }
        public string File { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string OrganizationCode { get; set; }
        public DateTime? DateAdd { get; set; }
        public DateTime? DateEdit { get; set; }

        // GitHub
        public string? GitHubOwner { get; set; } // org або user
        public string? GitHubRepoName { get; set; } // назва репозиторію
    }
}
