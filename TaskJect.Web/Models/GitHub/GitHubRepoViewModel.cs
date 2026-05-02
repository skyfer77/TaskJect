namespace TaskJect.Web.Models
{
    public class GitHubRepoViewModel
    {
        public long InstallationId { get; set; }
        public Guid? ProjectId { get; set; }
        public List<RepoItem> Repositories { get; set; } = new();
        public string CurrentRepoFullName { get; set; }

        public class RepoItem
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public string FullName { get; set; }
        }
    }
}
