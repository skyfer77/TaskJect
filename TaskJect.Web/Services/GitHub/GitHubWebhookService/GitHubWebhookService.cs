using Domain.Database;
using TaskJect.Web.Models;
using Data.Database.Repository;
using Domain.Database;
using System.Text.Json;
using Task = System.Threading.Tasks.Task;

namespace TaskJect.Web.Services
{
    public class GitHubWebhookService : IGitHubWebhookService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IGitHubAppService _gitHubAppService;
        private readonly FullUnlinkGitHubByTransaction _fullUnlinkGitHubByTransaction;

        public GitHubWebhookService(IOrganizationRepository organizationRepository, IProjectRepository projectRepository,
            IGitHubAppService gitHubAppService, ITaskRepository taskRepository, FullUnlinkGitHubByTransaction fullUnlinkGitHubByTransaction)
        {
            _organizationRepository = organizationRepository;
            _projectRepository = projectRepository;
            _gitHubAppService = gitHubAppService;
            _taskRepository = taskRepository;
            _fullUnlinkGitHubByTransaction = fullUnlinkGitHubByTransaction;
        }

        public async Task HandleEvent(string eventType, string payload)
        {
            switch (eventType)
            {
                case "repository":
                    await handleRepositoryEvent(payload);
                    break;

                case "organization":
                    await handleOrganizationEvent(payload);
                    break;

                case "installation":
                    await handleInstallationEvent(payload);
                    break;

                case "installation_repositories":
                    await handleInstallationRepositoriesEvent(payload);
                    break;

                case "pull_request":
                    await handlePullRequestEvent(payload);
                    break;

                //Для гілок нема окремих івентів в GitHub
                case "delete":
                    await handleBranchEvent(payload, "delete");
                    break;

                default:
                    Console.WriteLine($"Unhandled event: {eventType}");
                    break;
            }
        }

        private async Task handleRepositoryEvent(string payload)
        {
            var json = JsonDocument.Parse(payload);
            var root = json.RootElement;

            var action = getStringSafe(root, "action");
            if (string.IsNullOrEmpty(action))
            {
                return;
            }

            if (!root.TryGetProperty("repository", out var repository))
            {
                return;
            }

            var owner = getStringSafe(repository.GetProperty("owner"), "login");
            var repoName = getStringSafe(repository, "name");

            switch (action)
            {
                case "renamed":
                    await handleRepoRenamed(root, owner, repoName);
                    break;

                case "transferred":
                    await handleRepoTransferred(root, owner, repoName);
                    break;

                case "deleted":
                    await handleRepoDeleted(owner, repoName);
                    break;

                default:
                    Console.WriteLine($"Unhandled repo event: {action}");
                    break;
            }
        }

        private async Task handleRepoRenamed(JsonElement root, string owner, string newRepoName)
        {
            if (!root.TryGetProperty("changes", out var changes) ||
                !changes.TryGetProperty("repository", out var repository) ||
                !repository.TryGetProperty("name", out var name) ||
                !name.TryGetProperty("from", out var from))
            {
                return;
            }

            var oldName = from.GetString();
            if (string.IsNullOrEmpty(oldName))
            {
                return;
            }

            var repo = new GitHubUpdateRepo
            {
                RepoName = oldName,
                NewRepoName = newRepoName,
                Owner = owner
            };

            var projectId = await _projectRepository.UpdateRepoName(repo);
            if (projectId is not null)
            {
                repo.ProjectId = projectId.Value;
                await _taskRepository.UpdateRepoNameByRepos(repo);
            }
        }

        private async Task handleRepoTransferred(JsonElement root, string owner, string repoName)
        {
            var installationId = getLongSafe(root.GetProperty("installation"), "id");
            if (installationId == null)
            {
                return;
            }

            var isAccessible = await _gitHubAppService.CheckRepoAccess(installationId.Value, owner, repoName);

            if (isAccessible)
            {
                //TODO: Якщо буде в організації TaskJect декілька owner GitHub то треба реалізувати зміну owner на проекті.
            }
            else
            {
                var projectId = await _projectRepository.RetrieveProjectId(repoName, owner);
                await _fullUnlinkGitHubByTransaction.UnlinkGitHubProject(projectId);
            }
        }

        private async Task handleRepoDeleted(string owner, string repoName)
        {
            var projectId = await _projectRepository.RetrieveProjectId(repoName, owner);
            await _fullUnlinkGitHubByTransaction.UnlinkGitHubProject(projectId);
        }

        private async Task handleOrganizationEvent(string payload)
        {
            var json = JsonDocument.Parse(payload);
            var root = json.RootElement;

            var action = getStringSafe(root, "action");
            if (string.IsNullOrEmpty(action))
            {
                return;
            }

            if (!root.TryGetProperty("organization", out var org))
            {
                return;
            }

            var newOwner = getStringSafe(org, "login");
            var installationId = getLongSafe(org, "id");

            if (installationId == null)
            {
                return;
            }

            var organizationId = await _organizationRepository.GetIdByInstallationId(installationId.Value);
            if (organizationId == null)
            {
                return;
            }

            switch (action)
            {
                case "renamed":
                    await handleOrganizationRenamed(root, organizationId, newOwner);
                    break;

                case "deleted":
                    await handleOrganizationDeleted(organizationId);
                    break;

                default:
                    Console.WriteLine($"Unhandled organization action: {action}");
                    break;
            }
        }

        private async Task handleOrganizationRenamed(JsonElement root, Guid? organizationId, string newOwner)
        {
            if (organizationId == null)
            {
                return;
            }

            if (!root.TryGetProperty("changes", out var changes) ||
                !changes.TryGetProperty("login", out var login) ||
                !login.TryGetProperty("from", out var from))
            {
                return;
            }

            var oldOwner = from.GetString();
            if (string.IsNullOrEmpty(oldOwner))
            {
                return;
            }

            var repoProject = new GitHubUpdateRepo
            {
                OrganizationId = organizationId.Value,
                Owner = oldOwner,
                NewOwner = newOwner,
            };

            var projectIds = await _projectRepository.UpdateOwnerByOrganizationId(repoProject);

            if (projectIds.Count == 0)
            {
                return;
            }

            var repo = new GitHubUpdateRepo
            {
                ProjectIds = projectIds,
                Owner = oldOwner,
                NewOwner = newOwner,
            };

            await _taskRepository.UpdateOwnerByRepos(repo);
        }

        private async Task handleOrganizationDeleted(Guid? organizationId)
        {
            if (organizationId is null)
            {
                return;
            }

            await _fullUnlinkGitHubByTransaction.UnlinkGitHubOrganization(organizationId.Value);
        }

        //Uninstall the app via GitHub
        private async Task handleInstallationEvent(string payload)
        {
            var json = JsonDocument.Parse(payload);
            var root = json.RootElement;

            var action = getStringSafe(root, "action");

            if (action == "deleted")
            {
                if (!root.TryGetProperty("installation", out var installation))
                {
                    return;
                }

                var installationId = getLongSafe(installation, "id");
                if (installationId == null)
                {
                    return;
                }

                var organizationId = await _organizationRepository.GetIdByInstallationId(installationId.Value);
                if (organizationId != null)
                {
                    await _fullUnlinkGitHubByTransaction.UnlinkGitHubOrganization(organizationId.Value);
                }
            }
        }

        //Коли My App втратив доступ до репозиторію
        private async Task handleInstallationRepositoriesEvent(string payload)
        {
            var json = JsonDocument.Parse(payload);
            var root = json.RootElement;

            var action = getStringSafe(root, "action");

            if (action == "removed")
            {
                if (!root.TryGetProperty("repositories_removed", out var repos))
                {
                    return;
                }

                foreach (var repo in repos.EnumerateArray())
                {
                    var repoName = getStringSafe(repo, "name");
                    var owner = getStringSafe(repo.GetProperty("owner"), "login");

                    if (!string.IsNullOrEmpty(repoName) && !string.IsNullOrEmpty(owner))
                    {
                        var id = await _projectRepository.RetrieveProjectId(repoName, owner);
                        await _fullUnlinkGitHubByTransaction.UnlinkGitHubProject(id);
                    }
                }
            }
        }

        private async Task handlePullRequestEvent(string payload)
        {
            var json = JsonDocument.Parse(payload);
            var root = json.RootElement;

            var action = getStringSafe(root, "action");
            if (string.IsNullOrEmpty(action))
            {
                return;
            }

            if (!root.TryGetProperty("pull_request", out var pr) ||
                !root.TryGetProperty("repository", out var repo) ||
                !root.TryGetProperty("installation", out var installation))
            {
                return;
            }

            var owner = getStringSafe(repo.GetProperty("owner"), "login");
            var repoName = getStringSafe(repo, "name");
            var branch = getStringSafe(pr.GetProperty("head"), "ref");
            var merged = getBoolSafe(pr, "merged");
            var installationId = getLongSafe(installation, "id");

            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repoName) || string.IsNullOrEmpty(branch))
            {
                return;
            }

            var updateStatus = new GitHubUpdateStatusTask
            {
                Owner = owner,
                RepoName = repoName,
                Branch = branch,
            };

            switch (action)
            {
                case "opened":
                case "reopened":
                    updateStatus.Status = Domain.Enums.TaskStatus.OnReview;
                    break;

                case "closed":
                    updateStatus.Status = merged.Value ? Domain.Enums.TaskStatus.Done : Domain.Enums.TaskStatus.InProgress;
                    break;
                //коли в відкритий PR пушать нові коміти
                case "synchronize":
                    updateStatus.Status = Domain.Enums.TaskStatus.OnReview;
                    break;

                default:
                    Console.WriteLine($"Unhandled Pull Request action: {action}");
                    break;
            }

            await _taskRepository.UpdateTaskStatusByBranch(updateStatus);

            // Оновлення GitHub Issue
            var issueNumber = await _taskRepository.GetIssueNumberByBranch(branch, owner, repoName);
            if (issueNumber.HasValue)
            {
                var issueState = updateStatus.Status == Domain.Enums.TaskStatus.Done ? "closed" : "open";
                var gitHubInfo = new GitHubInfo()
                {
                    InstallationId = installationId,
                    Owner = owner,
                    RepoName = repoName,
                    GitHubIssueNumber = issueNumber,
                };
                await _gitHubAppService.UpdateIssueState(gitHubInfo, issueState);
            }
        }

        private async Task handleBranchEvent(string payload, string action)
        {
            var json = JsonDocument.Parse(payload);
            var root = json.RootElement;

            var refType = getStringSafe(root, "ref_type");
            if (refType != "branch")
            {
                return;
            }

            var branchName = getStringSafe(root, "ref");
            if (string.IsNullOrEmpty(branchName))
            {
                return;
            }

            if (!root.TryGetProperty("repository", out var repo))
            {
                return;
            }

            var owner = getStringSafe(repo.GetProperty("owner"), "login");
            var repoName = getStringSafe(repo, "name");

            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repoName))
            {
                return;
            }

            if (action == "delete")
            {
                await _taskRepository.DeleteBranch(owner, repoName, branchName);
            }
        }

        private string? getStringSafe(JsonElement element, string propertyName)
            => element.TryGetProperty(propertyName, out var value) ? value.GetString() : null;

        private long? getLongSafe(JsonElement element, string propertyName)
            => element.TryGetProperty(propertyName, out var value) && value.TryGetInt64(out var result) ? result : null;

        private bool? getBoolSafe(JsonElement element, string propertyName)
            => element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.True ? true :
               element.TryGetProperty(propertyName, out value) && value.ValueKind == JsonValueKind.False ? false : null;
    }
}