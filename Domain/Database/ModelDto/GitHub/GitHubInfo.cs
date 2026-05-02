namespace Domain.Database
{
    public class GitHubInfo
    {
        public long? InstallationId { get; set; }
        public string? Owner { get; set; }
        public string? RepoName { get; set; }
        public string? BranchName { get; set; }
		public int? GitHubIssueNumber { get; set; }
    }
}
