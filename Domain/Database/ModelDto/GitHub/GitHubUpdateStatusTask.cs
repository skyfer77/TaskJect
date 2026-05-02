namespace Domain.Database
{
    public class GitHubUpdateStatusTask
    {
        public string? Owner { get; set; }
        public string? RepoName { get; set; }
        public string? Branch { get; set; }
        public Domain.Enums.TaskStatus Status { get; set; }
    }
}
