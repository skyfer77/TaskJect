namespace Domain.Database
{
    public class LinkRepoModel
    {
        public Guid ProjectId { get; set; }
        public long InstallationId { get; set; }
        public string RepoFullName { get; set; }
    }
}
