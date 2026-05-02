namespace Domain.Database
{
    public class GitHubUpdateRepo
    {
        public Guid OrganizationId { get; set; }
        public Guid ProjectId { get; set; }
        public List<Guid> ProjectIds { get; set; }
        public string? RepoName { get; set; }
        public string? NewRepoName { get; set; }
        public string? Owner { get; set; }
        public string? NewOwner { get; set; }
    }
}
