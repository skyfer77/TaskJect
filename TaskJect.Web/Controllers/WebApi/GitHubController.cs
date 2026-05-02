using TaskJect.Web.Services;
using Microsoft.AspNetCore.Mvc;
using TaskJect.Web.Models;
using Domain.Database;
using TaskJect.Web.Resources;
using Microsoft.Extensions.Localization;
using Data.Database.Repository;

namespace TaskJect.Web.Controllers
{
    [Route("github")]
    public class GitHubController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IGitHubAppService _gitHubAppService;
        private readonly IProjectRepository _projectRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly FullUnlinkGitHubByTransaction _fullUnlinkGitHubByTransaction;
        private readonly IStringLocalizer<ErrorResources> _localizer;

        public GitHubController(IConfiguration config, IGitHubAppService gitHubAppService, IProjectRepository projectRepository,
            IOrganizationRepository organizationRepository, FullUnlinkGitHubByTransaction fullUnlinkGitHubByTransaction,
            IStringLocalizer<ErrorResources> localizer)
        {
            _config = config;
            _gitHubAppService = gitHubAppService;
            _projectRepository = projectRepository;
            _organizationRepository = organizationRepository;
            _fullUnlinkGitHubByTransaction = fullUnlinkGitHubByTransaction;
            _localizer = localizer;
        }

        [HttpGet("install")]
        public IActionResult InstallApp()
        {
            var organizationCode = this.GetOrganizationCode();
            var appSlug = _config["GitHub:AppName"];
            var url = $"https://github.com/apps/{appSlug}/installations/new?state={organizationCode}";
            return Redirect(url);
        }

        // Callback після встановлення
        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] long installation_id, [FromQuery] string state)
        {
            if (!Guid.TryParse(state, out var organizationCode))
            {
                return BadRequest("Invalid organizationCode");
            }

            await _organizationRepository.SetGitHubInstallationId(organizationCode, installation_id);

            return Redirect($"/Organization/Index");
        }

        [HttpPost("unlink")]
        public async Task<IActionResult> Unlink()
        {
            var organizationCode = this.GetOrganizationCode();
            var organizationId = Guid.Parse(organizationCode);
            var installationId = await _organizationRepository.FindGitHubInstallationId(organizationId);

            if (installationId == null)
            {
                return Json(new ServerResponse(false) { Message = _localizer["GitHubNotLinked"] });
            }

            var result = await _gitHubAppService.DeleteInstallationGitHubAsync(installationId.Value);
            if (!result)
            {
                return Json(new ServerResponse(false) { Message = _localizer["GitHubNotDeleteApp"] });
            }

            await _fullUnlinkGitHubByTransaction.UnlinkGitHubOrganization(organizationId);

            return Redirect("/Organization/Index");
        }

        [HttpGet("repos")]
        public async Task<IActionResult> GetRepositories([FromQuery] Guid? projectId)
        {
            var organizationCode = this.GetOrganizationCode();
            var installationId = await _organizationRepository.FindGitHubInstallationId(Guid.Parse(organizationCode));
            if (installationId == null)
            {
                return BadRequest();
            }

            var repos = await _gitHubAppService.GetRepositoriesAsync(installationId.Value);

            var currentRepoFullName = await _projectRepository.GetCurrentRepoFullName(projectId.Value);

            var model = new GitHubRepoViewModel
            {
                InstallationId = installationId.Value,
                ProjectId = projectId,
                Repositories = repos,
                CurrentRepoFullName = currentRepoFullName
            };

            var html = await this.RenderViewAsync("_SelectRepo", model);

            return Json(new ServerResponse(true)
            {
                Html = html,
            });
        }

        [HttpPost("linkRepoToProject")]
        public async Task<IActionResult> LinkRepoToProject(LinkRepoModel model)
        {
            if (string.IsNullOrEmpty(model.RepoFullName))
            {
                return BadRequest(_localizer["NoRepositorySelected"]);
            }

            var parts = model.RepoFullName.Split('/');
            var owner = parts[0];
            var repo = parts[1];

            var linkRepo = new ProjectDto
            {
                ID = model.ProjectId,
                GitHubOwner = owner,
                GitHubRepoName = repo
            };

            await _projectRepository.UpdateGitHubInfo(linkRepo);

            var managerId = await _projectRepository.RetrieveManagerId(model.ProjectId);
            var userId = this.GetUserId();

            ViewBag.GrantAccessRole = User.IsInRole("Admin") || User.IsInRole("God")
                || User.IsInRole("TeamLead") || managerId == userId;
            return PartialView("_GitHubRepoInfo", linkRepo);
        }

        [HttpPost("unlinkRepoToProject")]
        public async Task<IActionResult> UnlinkRepoToProject(Guid projectId)
        {
            if (projectId == Guid.Empty)
            {
                return BadRequest(_localizer["MissingProjectId"]);
			}

			var projectInfo = await _projectRepository.FindGitHubInfo(projectId);
			if (projectInfo == null)
			{
				return NotFound(_localizer["ProjectNotFound"]);
			}

			if (string.IsNullOrEmpty(projectInfo.Owner) && string.IsNullOrEmpty(projectInfo.RepoName))
			{
				return BadRequest(_localizer["ProjectHasNoLinkedRepository"]);
			}

			var unlinkRepo = new ProjectDto
            {
                ID = projectId,
                GitHubOwner = null,
                GitHubRepoName = null
            };

            await _fullUnlinkGitHubByTransaction.UnlinkGitHubProject(projectId);

            ViewBag.ShowSelect = true;
            return PartialView("_GitHubRepoInfo", unlinkRepo);
        }
    }
}