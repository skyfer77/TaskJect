using TaskJect.Web.Models;
using Domain.Database;

namespace TaskJect.Web.Services
{
    public interface IGitHubAppService
    {
        Task<GitHubInstallationToken> GetInstallationTokenAsync(long installationId);
        Task<bool> DeleteInstallationGitHubAsync(long installationId);
        Task<List<GitHubRepoViewModel.RepoItem>> GetRepositoriesAsync(long installationId);
        Task<bool> CreateBranch(GitHubCreateBranch model);
        Task<bool> BranchExists(GitHubInfo model);
		Task<int?> CreateIssue(GitHubCreateIssue model);
        Task<bool> CheckIssueExists(GitHubInfo model);
        Task<bool> UpdateIssueState(GitHubInfo model, string issueState);
        Task<bool> CheckRepoAccess(long installationId, string? owner, string? repoName);
    }

    public class GitHubInstallationToken
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
