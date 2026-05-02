using TaskJect.Web.Models;
using Domain.Database;
using Task = System.Threading.Tasks.Task;

namespace TaskJect.Web.Services
{
    public class FakeGitHubAppService : IGitHubAppService
    {
        public Task<GitHubInstallationToken> GetInstallationTokenAsync(long installationId)
            => Task.FromResult<GitHubInstallationToken>(null);

        public Task<bool> DeleteInstallationGitHubAsync(long installationId)
            => Task.FromResult(true);

        public Task<List<GitHubRepoViewModel.RepoItem>> GetRepositoriesAsync(long installationId)
            => Task.FromResult(new List<GitHubRepoViewModel.RepoItem>());

        public Task<bool> CreateBranch(GitHubCreateBranch model)
            => Task.FromResult(true);

        public Task<bool> BranchExists(GitHubInfo model)
			=> Task.FromResult(true);

		public Task<int?> CreateIssue(GitHubCreateIssue model)
            => Task.FromResult<int?>(null);

        public Task<bool> CheckIssueExists(GitHubInfo model)
            => Task.FromResult(false);

        public Task<bool> UpdateIssueState(GitHubInfo model, string issueState)
            => Task.FromResult(true);

        public Task<bool> CheckRepoAccess(long installationId, string? owner, string? repoName)
            => Task.FromResult(true);
    }
}
