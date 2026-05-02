namespace Domain.Database
{
    public class GitHubCreateIssue
    {
        public GitHubInfo GitHubInfo { get; set; }
        public bool CreateNewIssue { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
