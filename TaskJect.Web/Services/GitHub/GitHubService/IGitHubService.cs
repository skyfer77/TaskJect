using TaskJect.Web.Models;
using Domain.Database;

namespace TaskJect.Web.Services
{
	public interface IGitHubService
	{
		Task<GitHubInfo?> CreateGitHubBranchAndIssue(BranchIssueGitHubParams gitHubParams);
		Task<bool> CreateBranchGitHub(BranchIssueGitHubParams gitHubParams);
		Task<bool> LinkGitHubIssue(BranchIssueGitHubParams link);
	}
}
