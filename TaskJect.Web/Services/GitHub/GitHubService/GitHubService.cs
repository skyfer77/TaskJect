using TaskJect.Web.Enums;
using TaskJect.Web.Models;
using Domain.Database;

namespace TaskJect.Web.Services
{
	public class GitHubService : IGitHubService
	{
		private readonly IGitHubAppService _gitHubAppService;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly IProjectRepository _projectRepository;

		public GitHubService(IGitHubAppService gitHubAppService, IOrganizationRepository organizationRepository,
			IProjectRepository projectRepository) 
		{ 
			_gitHubAppService = gitHubAppService;
			_organizationRepository = organizationRepository;
			_projectRepository = projectRepository;
		}

		public async Task<GitHubInfo?> CreateGitHubBranchAndIssue(BranchIssueGitHubParams gitHubParams)
		{
			var installationId = await _organizationRepository.FindGitHubInstallationId(gitHubParams.OrganizationId);
			var gitHubInfo = await _projectRepository.FindGitHubInfo(gitHubParams.ProjectId);

			if (gitHubInfo != null && installationId != null)
			{
				gitHubInfo.InstallationId = installationId;
				if (string.IsNullOrEmpty(gitHubInfo.Owner) || string.IsNullOrEmpty(gitHubInfo.RepoName))
				{
					return null;
				}

				if (gitHubParams.TaskStatus == TaskStatusView.InProgress
					&& !string.IsNullOrWhiteSpace(gitHubParams.NewBranchName))
				{
					gitHubInfo.BranchName = gitHubParams.NewBranchName;
					var result = await createBranchGitHub(gitHubInfo);
					if (result)
					{
						gitHubInfo.BranchName = gitHubParams.NewBranchName;
					}
				}

				gitHubInfo.GitHubIssueNumber = gitHubParams.GitHubIssueNumber;
				var gitHubIssue = new GitHubCreateIssue
				{
					GitHubInfo = gitHubInfo,
					CreateNewIssue = gitHubParams.CreateNewIssue,
					Title = gitHubParams.TitleTask,
					Body = gitHubParams.DescriptionTask
				};
				var issueNumber = await _gitHubAppService.CreateIssue(gitHubIssue);

				if (issueNumber != null)
				{
					gitHubInfo.GitHubIssueNumber = issueNumber;
				}

				return gitHubInfo;
			}

			return null;
		}

		public async Task<bool> CreateBranchGitHub(BranchIssueGitHubParams gitHubParams)
		{
			var gitHubInfo = await _projectRepository.FindGitHubInfo(gitHubParams.ProjectId);
			if (gitHubInfo == null ||
				string.IsNullOrWhiteSpace(gitHubInfo.Owner) ||
				string.IsNullOrWhiteSpace(gitHubInfo.RepoName))
			{
				return false;
			}

			var installationId = await _organizationRepository.FindGitHubInstallationId(gitHubParams.OrganizationId);
			if (installationId != null && !string.IsNullOrWhiteSpace(gitHubParams.NewBranchName))
			{
				gitHubInfo.InstallationId = installationId;
				gitHubInfo.BranchName = gitHubParams.NewBranchName;

				var exists = await _gitHubAppService.BranchExists(gitHubInfo);
				if (exists)
				{
					return true;
				}

				var result = await createBranchGitHub(gitHubInfo);

				return result;
			}

			return false;
		}

		private async Task<bool> createBranchGitHub(GitHubInfo gitHubInfo)
		{
			if (gitHubInfo.BranchName == null 
				|| string.IsNullOrEmpty(gitHubInfo.BranchName))
			{
				return false;
			}

			var gitHubBranch = new GitHubCreateBranch
			{
				GitHubInfo = gitHubInfo,
				NewBranchName = gitHubInfo.BranchName,
			};
			var result = await _gitHubAppService.CreateBranch(gitHubBranch);
			return result;
		}

		public async Task<bool> LinkGitHubIssue(BranchIssueGitHubParams link)
		{
			var gitHubInfo = await _projectRepository.FindGitHubInfo(link.ProjectId);
			var installationId = await _organizationRepository.FindGitHubInstallationId(link.OrganizationId);

			if (gitHubInfo != null && installationId != null)
			{
				gitHubInfo.InstallationId = installationId;
				gitHubInfo.GitHubIssueNumber = link.GitHubIssueNumber;

				var exists = await _gitHubAppService.CheckIssueExists(gitHubInfo);
				if (exists)
				{
					return true;
				}
			}

			return false;
		}
	}
}
