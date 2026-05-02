using TaskJect.Web.Models;
using TaskJect.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskJect.Web.Resources;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using System.Text.Json;
using Domain.Database;
using Domain.IServices;
using Data.Database.Repository;

namespace TaskJect.Web.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IProjectRepository _projectRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IOrganizationStorageChecker _organizationStorageChecker;
        private readonly ITariffPlanHistoryRepository _tariffPlanHistoryRepository;
        private readonly IProjectUserPermissionRepository _projectUserPermissionRepository;
		private readonly IOrganizationFilesRepository _organizationFilesRepository;
		private readonly IStringLocalizer<ErrorResources> _localizer;
        private readonly ITariffPlanRepository _tariffPlanRepository;
        private readonly IOrganizationRepository _organizationRepository;

        public ProjectController(IHttpClientFactory httpClientFactory, IApplicationUserRepository applicationUserRepository,
           IProjectRepository projectRepository, ITaskRepository taskRepository, IMembershipRepository membershipRepository,
           ITeamRepository teamRepository, IOrganizationStorageChecker organizationStorageChecker,
           ITariffPlanHistoryRepository tariffPlanHistoryRepository, IProjectUserPermissionRepository projectUserPermissionRepository,
		   IOrganizationFilesRepository organizationFilesRepository, ITariffPlanRepository tariffPlanRepository,
           IOrganizationRepository organizationRepository,
           IStringLocalizer<ErrorResources> localizer)
        {
            _httpClientFactory = httpClientFactory;
            _applicationUserRepository = applicationUserRepository;
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
            _membershipRepository = membershipRepository;
            _teamRepository = teamRepository;
            _organizationStorageChecker = organizationStorageChecker;
            _tariffPlanHistoryRepository = tariffPlanHistoryRepository;
            _projectUserPermissionRepository = projectUserPermissionRepository;
			_organizationFilesRepository = organizationFilesRepository;
            _tariffPlanRepository = tariffPlanRepository;
            _organizationRepository = organizationRepository;
            _localizer = localizer;
        }
        public async Task<IActionResult> Index()
        {
            var organizationCode = this.GetOrganizationCode();
            var activePlan = await _tariffPlanHistoryRepository.RetrieveActive(Guid.Parse(organizationCode));
            var currentPlan = await _tariffPlanRepository.Retrieve(activePlan.TariffPlanCode);
            ViewBag.HasProjectAccessControl = currentPlan.HasProjectAccessControl;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _applicationUserRepository.GetUserById(userId, organizationCode);
            if (user == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }
            var memberships = await _membershipRepository.GetMembershipsByUser(userId);
            if (memberships == null)
            {
                return redirectToErrorPage(_localizer["membershipsByUserLoadErrorTitle"], _localizer["membershipsByUserLoadErrorMessage"]);
            }
            List<Guid> teamIds = new List<Guid>();
            var projectsByTeam = new Dictionary<Guid, List<ProjectDto>>();
            var projectsById = new Dictionary<Guid, ProjectDto>();
            teamIds = memberships.Select(m => m.TeamId).Distinct().ToList();
            if (teamIds.Any())
            {
                projectsByTeam = await _projectRepository.RetrieveByTeamsIDs(teamIds);
                if (projectsByTeam != null && projectsByTeam.Any())
                {
                    projectsById = projectsByTeam.SelectMany(pr => pr.Value).ToDictionary(p => p.ID.Value, p => p);

                    if (projectsById != null && projectsById.Any())
                    {
                        var totalTasks = _taskRepository.GetTaskProgress(projectsById.Keys);
                        foreach (var project in projectsById.Values)
                        {
                            var team = await _teamRepository.GetTeamById(project.TeamId.Value);
                            if (team != null)
                            {
                                project.Team = team;
                            }
                            else
                            {
                                return redirectToErrorPage(_localizer["teamOverviewLoadErrorTitle"], _localizer["teamOverviewLoadErrorMessage"]);
                            }
                            if (totalTasks.TryGetValue(project.ID.Value, out var tasks))
                            {
                                project.TaskTotal = tasks.TotalTasks;
                                project.TaskCompleted = tasks.CompletedTasks;
                                if (project.TaskTotal != 0 || project.TaskCompleted != 0)
                                {
                                    var status = ((double)project.TaskCompleted / (double)project.TaskTotal) * 100;
                                    project.StatusCompleted = (int)status;
                                }
                            }
                        }
                    }
                    else if (projectsById == null)
                    {
                        return redirectToErrorPage(_localizer["projectsByOrganizationLoadErrorTitle"], _localizer["projectsByOrganizationLoadErrorMessage"]);
                    }
                }
            }
            return View(projectsById.Values);
        }

        public async Task<IActionResult> Create()
        {
            var organizationCode = this.GetOrganizationCode();
            var organizationId = Guid.Parse(organizationCode);


            var activePlan = await _tariffPlanHistoryRepository.RetrieveActive(organizationId);

            var currentPlan = await _tariffPlanRepository.Retrieve(activePlan.TariffPlanCode);
            ViewBag.HasProjectAccessControl = currentPlan.HasProjectAccessControl;
            ViewBag.HasGitHubIntegration = currentPlan.HasGitHubIntegration;

            var users = await _applicationUserRepository.GetAllUsersTheOrganization(organizationCode);

            if (users == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }

            var memberships = await _membershipRepository.GetMemberships();

            if (memberships == null)
            {
                return redirectToErrorPage(_localizer["allMembershipsLoadErrorTitle"], _localizer["allMembershipsLoadErrorMessage"]);
            }

            var teams = await _teamRepository.GetTeamsByOrganization(organizationCode);

            if (teams == null)
            {
                return redirectToErrorPage(_localizer["teamsByOrganizationLoadErrorTitle"], _localizer["teamsByOrganizationLoadErrorMessage"]);
            }

			//The project manager does not exist
			ViewBag.IsProjectManager = false;

			var installationId = await _organizationRepository.FindGitHubInstallationId(organizationId);
			ViewBag.IsSetGitRepo = installationId != null;

			if (!teams.Any())
            {
                ViewBag.NoTeams = true;
                return View();
            }

            var teamsWithMembers = TeamsWithMembers(users, memberships, teams);
            if (teamsWithMembers.Count == 0)
            {
                ViewBag.NoTeams = true;
                return View();
            }
            var firstTeamEntry = teamsWithMembers.FirstOrDefault();
            
            var usersFirstTeam = firstTeamEntry.Value;

            var teamsMembers = new CreateProjectPageModel()
            {
                TeamWithUsers = teamsWithMembers,
                PermissionsUsers = getProjectPermissionsForUsers(usersFirstTeam)
            };

            return View(teamsMembers);
        }

        [HttpPost]
        public async Task<JsonResult> CreateProject(ProjectViewModel model)
        {
            if (model != null)
            {
                var organizationCode = this.GetOrganizationCode();
                if (!await _organizationStorageChecker.CheckAsync(Guid.Parse(organizationCode)))
                {
                    return Json(new ServerResponse(false) { Message = _localizer["StorageYourOrganizationFull"] });
                }
                model.Project = setGitHubOwnerAndRepo(model);
                model.Project.OrganizationCode = organizationCode;
                model.Project.DateAdd = DateTime.Now;
                //need for permissions Users
                model.Project.ID = Guid.NewGuid();
                var result = await _projectRepository.Insert(model.Project);
                if(result)
                {
                    var activePlan = await _tariffPlanHistoryRepository.RetrieveActive(Guid.Parse(organizationCode));
                    var tariffPlan = await _tariffPlanRepository.Retrieve(activePlan.TariffPlanCode);
                    var permissionsUsers = setProjectId(model.ProjectUserPermissions, model.Project.ID.Value);
                    if (activePlan != null && tariffPlan.HasProjectAccessControl)
                    {
                        var resultInsertPermissions = await _projectUserPermissionRepository.Update(permissionsUsers);
                    }
					//permissions users default set by Trigger

					return Json(new ServerResponse(result) { Message = _localizer["YourOperationSuccessful"], ProjectId = model.Project.ID });
                }
                else
                {
                    return Json(new ServerResponse(result) { Message = _localizer["YourOperationWasNotSuccessful"] });
                }
            }
            else
            {
                return Json(new ServerResponse(false) { Message = _localizer["IdProjectEqualsNull"] });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTeamSelector(Guid TeamId)
        {
            var memberships = await _membershipRepository.GetMemberships();
            if (memberships == null)
            {
                return redirectToErrorPage(_localizer["allMembershipsLoadErrorTitle"], _localizer["allMembershipsLoadErrorMessage"]);
            }

            var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;
            var users = await _applicationUserRepository.GetAllUsersTheOrganization(organizationCode);
            if (users == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }

            var userList = getUsersByTeam(TeamId, users, memberships);

            return Json(userList);
        }

        [HttpPost]
        public async Task<IActionResult> LoadPermissionsTable(Guid teamId, Guid? projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var memberships = await _membershipRepository.GetMemberships();
            if (memberships == null)
            {
                return redirectToErrorPage(_localizer["allMembershipsLoadErrorTitle"], _localizer["allMembershipsLoadErrorMessage"]);
            }

            var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;
            var activePlan = await _tariffPlanHistoryRepository.RetrieveActive(Guid.Parse(organizationCode));

            var currentPlan = await _tariffPlanRepository.Retrieve(activePlan.TariffPlanCode);
            ViewBag.HasProjectAccessControl = currentPlan.HasProjectAccessControl;

            var users = await _applicationUserRepository.GetAllUsersTheOrganization(organizationCode);
            if (users == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }

            var userList = getUsersByTeam(teamId, users, memberships);

            List<ProjectUserPermissionDto> permissions = null;

            ViewBag.IsProjectManager = false;

            var managerId = "";
            if (projectId.HasValue && projectId.Value != Guid.Empty)
            {
                permissions = await _projectUserPermissionRepository.Retrieve(projectId.Value);

                managerId = await _projectRepository.RetrieveManagerId(projectId.Value);
                ViewBag.IsProjectManager = managerId == userId;
            }

            var permissionViewModel = getProjectPermissionsForUsers(userList, managerId, permissions);
            return PartialView("_TableProjectUserPermission", permissionViewModel);
        }

        [HttpPost]
        public async Task<ActionResult> EditProject(Guid id)
        {
            var organizationCode = this.GetOrganizationCode();
            var userId = this.GetUserId();
            var organizationId = Guid.Parse(organizationCode);
            var activePlan = await _tariffPlanHistoryRepository.RetrieveActive(organizationId);

            var currentPlan = await _tariffPlanRepository.Retrieve(activePlan.TariffPlanCode);
            ViewBag.HasProjectAccessControl = currentPlan.HasProjectAccessControl;
            ViewBag.HasGitHubIntegration = currentPlan.HasGitHubIntegration;

            var users = await _applicationUserRepository.GetAllUsersTheOrganization(organizationCode);

            if (users == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }

            var memberships = await _membershipRepository.GetMemberships();
            if (memberships == null)
            {
                return redirectToErrorPage(_localizer["allMembershipsLoadErrorTitle"], _localizer["allMembershipsLoadErrorMessage"]);
            }

            var teams = await _teamRepository.GetTeamsByOrganization(organizationCode);
            if (teams == null)
            {
                return redirectToErrorPage(_localizer["teamsByOrganizationLoadErrorTitle"], _localizer["teamsByOrganizationLoadErrorMessage"]);
            }

            var teamsWithMembers = TeamsWithMembers(users, memberships, teams);
            if (teamsWithMembers.Count() == 0)
            {
                return Json(new ServerResponse(false) { Message = _localizer["TeamMembersNull"] });
            }

            var project = await _projectRepository.Retrieve(id);
            if (project == null)
            {
                return redirectToErrorPage(_localizer["projectOverviewLoadErrorTitle"], _localizer["projectOverviewLoadErrorMessage"]);
            }

            var projectPermissionsUsers = await _projectUserPermissionRepository.Retrieve(id);

			var projectFiles = await _organizationFilesRepository.RetrieveLightProjectFile(id);

			var userList = getUsersByTeam(project.TeamId.Value, users, memberships);

			ViewBag.IsProjectManager = project.ManagerID == userId;
            var installationId = await _organizationRepository.FindGitHubInstallationId(organizationId);
            ViewBag.IsSetGitRepo = installationId != null;

            var taskProject = new EditProjectModel()
            {
                Project = project,
                TeamsWithMembers = teamsWithMembers,
                PermissionsUsers = getProjectPermissionsForUsers(userList, project.ManagerID, projectPermissionsUsers),
                OrganizationFiles = projectFiles.ToList()

			};

            return PartialView("_EditProject", taskProject);
        }

        [HttpPost]
        public async Task<JsonResult> Edit(ProjectViewModel model, string filesToDelete)
        {
            if (model != null)
            {
                var organizationCode = this.GetOrganizationCode();
                model.Project.OrganizationCode = organizationCode;

                if (!await _organizationStorageChecker.CheckAsync(Guid.Parse(organizationCode)))
                {
                    return Json(new ServerResponse(false) { Message = _localizer["StorageYourOrganizationFull"] });
                }

                model.Project = setGitHubOwnerAndRepo(model);

                model.Project.DateEdit = DateTime.Now;
                var result = await _projectRepository.Update(model.Project);
                if (result)
                {
					var idsfilesToDelete = parseGuidsFromJson(filesToDelete);

					if (idsfilesToDelete != null && idsfilesToDelete.Count != 0)
					{
						await _organizationFilesRepository.DeleteFiles(idsfilesToDelete);
					}

					var permissionsUsers = setProjectId(model.ProjectUserPermissions, model.Project.ID.Value);
                    var resultUpdatePermissions = await _projectUserPermissionRepository.Update(permissionsUsers);

                    return Json(new ServerResponse(result) 
                    { 
                        Message = _localizer["YourOperationSuccessful"], 
                        ProjectId = model.Project.ID 
                    });
                }
                else
                {
                    return Json(new ServerResponse(result) { Message = _localizer["YourOperationWasNotSuccessful"] });
                }
            }
            else
            {
                return Json(new ServerResponse(false) { Message = _localizer["ModelProjectEqualsNull"] });
            }
        }

        private ProjectDto setGitHubOwnerAndRepo(ProjectViewModel model)
        {
            if (!string.IsNullOrEmpty(model.RepoFullName))
            {
                var parts = model.RepoFullName.Split('/');
                model.Project.GitHubOwner = parts[0];
                model.Project.GitHubRepoName = parts[1];
            }

            return model.Project;
        }

        private List<Guid> parseGuidsFromJson(string json)
		{
			var result = new List<Guid>();

			if (string.IsNullOrWhiteSpace(json))
			{
				return result;
			}

			var rawList = JsonSerializer.Deserialize<List<string>>(json);

			if (rawList != null)
			{
				foreach (var idStr in rawList)
				{
					if (Guid.TryParse(idStr, out var guid))
					{
						result.Add(guid);
					}
				}
			}

			return result;
		}

		[HttpPost]
        public async Task<IActionResult> UpdatePermissions(ProjectViewModel model)
        {
            if (model.ProjectUserPermissions != null && model.ProjectUserPermissions.Any())
            {
                var result = await _projectUserPermissionRepository.Update(model.ProjectUserPermissions);
                if (result)
                {
                    return Json(new ServerResponse(result) { Message = _localizer["YourOperationSuccessful"] });
                }
                else
                {
                    return Json(new ServerResponse(result) { Message = _localizer["YourOperationWasNotSuccessful"] });
                }
            }
            else
            {
                return Json(new ServerResponse(false) { Message = _localizer["ModelProjectEqualsNull"] });
            }
        }

        [HttpPost]
        public async Task<JsonResult> Delete(Guid id)
        {
            if (!id.Equals(null))
            {
				await _organizationFilesRepository.DeleteAllFileProject(id);
				await _projectUserPermissionRepository.Delete(id);

				var result = await _projectRepository.Delete(id);
                if (result)
                {
                    return Json(new ServerResponse(result) { Message = _localizer["YourOperationSuccessful"] });
                }
                else
                {
                    return Json(new ServerResponse(result) { Message = _localizer["YourOperationWasNotSuccessful"] });
                }
            }
            else
            {
                return Json(new ServerResponse(false) { Message = _localizer["IdProjectEqualsNull"] });
            }
        }

        private List<ApplicationUserLiteDto> getUsersByTeam(Guid teamId, List<ApplicationUserLiteDto> users,
            IEnumerable<MembershipDto> memberships)
        {
            var members = memberships.Where(x => x.TeamId == teamId).ToList();

            var usersList = new List<ApplicationUserLiteDto>();
            foreach (var member in members)
            {
                usersList.AddRange(users.Where(x => x.Id == member.UserId).ToList());
            }

            return usersList;
        }

        private List<ProjectUserPermissionDto> setProjectId(List<ProjectUserPermissionDto> permissions, Guid projectId)
        {
            if (permissions == null)
                return new List<ProjectUserPermissionDto>();

            foreach (var permission in permissions)
            {
                permission.ProjectId = projectId;
            }

            return permissions;
        }

        private List<ProjectPermissionForUser> getProjectPermissionsForUsers(List<ApplicationUserLiteDto> users, string managerId = "",
            List<ProjectUserPermissionDto> permissions = null)
        {
            var userPermissions = new List<ProjectPermissionForUser>();

            foreach (var user in users)
            {
                var existingPermission = permissions?.FirstOrDefault(p => p.UserId == user.Id);

				userPermissions.Add(new ProjectPermissionForUser
                {
                    UserName = user.Name,
                    UserSurname = user.Surname,
                    Role = user.Role,
                    IsProjectManager = user.Id == managerId,
					ProjectUserPermission = existingPermission ?? new ProjectUserPermissionDto
                    {
                        UserId = user.Id,
                        CanCreateTask = true,
                        CanEditTask = true,
                        CanDeleteTask = true,
                        CanAssignUsers = true
                    }
                });
            }

            return userPermissions;
        }

        private Dictionary<TeamDto, List<ApplicationUserLiteDto>> TeamsWithMembers(List<ApplicationUserLiteDto> users, IEnumerable<MembershipDto> memberships, IEnumerable<TeamDto> teams)
        {
            var usersIds = new HashSet<string?>(users.Select(x => x.Id));
            var teamsWithUsers = new Dictionary<TeamDto, List<ApplicationUserLiteDto>>();

            foreach (var team in teams)
            {
                var members = memberships.Where(membership => membership.TeamId == team.Id && usersIds.Contains(membership.UserId)).Select(membership => users.First(x => x.Id == membership.UserId)).ToList();
                if (members.Count > 0)
                {
                    teamsWithUsers.Add(team, members);
                }
            }

            return teamsWithUsers;
        }

        [Route("{controller=Project}/{action=Overview}/{id}")]
        public async Task<IActionResult> Overview(Guid id)
        {
            var organizationCode = this.GetOrganizationCode();
            var organizationId = Guid.Parse(organizationCode);
            var activePlan = await _tariffPlanHistoryRepository.RetrieveActive(organizationId);

            var currentPlan = await _tariffPlanRepository.Retrieve(activePlan.TariffPlanCode);
            ViewBag.HasProjectAccessControl = currentPlan.HasProjectAccessControl;
            ViewBag.HasGitHubIntegration = currentPlan.HasGitHubIntegration;

            var project = await _projectRepository.Retrieve(id);
            if (project.OrganizationCode != organizationCode)
            {
                return RedirectToAction("PageNotAccess", "Error");
			}
            if (project != null)
            {
                var user = await _applicationUserRepository.GetUserById(project.ManagerID, organizationCode);
                if (user != null)
                {
                    project.Manager = user;
                }
            }
            else if (project == null)
            {
                return redirectToErrorPage(_localizer["projectsByOrganizationLoadErrorTitle"], _localizer["projectsByOrganizationLoadErrorMessage"]);
            }

            var team = await _teamRepository.GetTeamById(project.TeamId.Value);
            if (team == null)
            {
                return redirectToErrorPage(_localizer["teamOverviewLoadErrorTitle"], _localizer["teamOverviewLoadErrorMessage"]);
            }

            var memberships = await _membershipRepository.GetMembershipByTeam(team.Id);
            if (memberships == null)
            {
                return redirectToErrorPage(_localizer["membershipsByUserLoadErrorTitle"], _localizer["membershipsByUserLoadErrorMessage"]);
            }

            var usersInProject = memberships.Select(x => x.UserId).ToList();

            var users = await _applicationUserRepository.GetUsersByIds(usersInProject, organizationCode);
            if (users == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }

			var projectFiles = await _organizationFilesRepository.RetrieveLightProjectFile(id);

			var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			ViewBag.IsProjectManager = project.ManagerID == userId;
            var installationId = await _organizationRepository.FindGitHubInstallationId(organizationId);
            ViewBag.IsSetGitRepo = installationId != null;
            var overviewModel = new OverviewModel()
            {
                Project = project,
                Team = team,
                User = users.Values.ToList(),
				OrganizationFiles = projectFiles.ToList()
			};

            return View(overviewModel);
        }

        [Route("{controller=Project}/{action=OverviewPermissionsUsers}/{id}")]
        public async Task<IActionResult> OverviewPermissionsUsers(Guid id)
        {
            var organizationCode = this.GetOrganizationCode();
            var userId = this.GetUserId();

            var activePlan = await _tariffPlanHistoryRepository.RetrieveActive(Guid.Parse(organizationCode));

            var currentPlan = await _tariffPlanRepository.Retrieve(activePlan.TariffPlanCode);
            ViewBag.HasProjectAccessControl = currentPlan.HasProjectAccessControl;
            //Якщо план не експерт
			if (!currentPlan.HasProjectAccessControl)
			{
				return RedirectToAction("PageNotAccess", "Error");
			}

			var users = await _applicationUserRepository.GetAllUsersTheOrganization(organizationCode);
            if (users == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }

            var memberships = await _membershipRepository.GetMemberships();
            if (memberships == null)
            {
                return redirectToErrorPage(_localizer["allMembershipsLoadErrorTitle"], _localizer["allMembershipsLoadErrorMessage"]);
            }

            var teams = await _teamRepository.GetTeamsByOrganization(organizationCode);
            if (teams == null)
            {
                return redirectToErrorPage(_localizer["teamsByOrganizationLoadErrorTitle"], _localizer["teamsByOrganizationLoadErrorMessage"]);
            }

            var project = await _projectRepository.Retrieve(id);
			if (project.OrganizationCode != organizationCode)
			{
				return RedirectToAction("PageNotAccess", "Error");
			}
			if (project == null)
            {
                return redirectToErrorPage(_localizer["projectOverviewLoadErrorTitle"], _localizer["projectOverviewLoadErrorMessage"]);
            }

            var projectPermissionsUsers = await _projectUserPermissionRepository.Retrieve(id);

			var userList = getUsersByTeam(project.TeamId.Value, users, memberships);

			ViewBag.IsProjectManager = project.ManagerID == userId;

            var taskProject = new OverviewPermissionsUsersModel()
            {
                Project = project,
                PermissionsUsers = getProjectPermissionsForUsers(userList, project.ManagerID, projectPermissionsUsers)
            };

            return View(taskProject);
        }

        [Route("{controller=Project}/{action=Analytics}/{id}")]
        public async Task<IActionResult> Analytics(Guid id)
        {
            var organizationCode = this.GetOrganizationCode();

			var project = await _projectRepository.Retrieve(id);
			if (project.OrganizationCode != organizationCode)
			{
				return RedirectToAction("PageNotAccess", "Error");
			}
			if (project == null)
            {
                return redirectToErrorPage(_localizer["projectOverviewLoadErrorTitle"], _localizer["projectOverviewLoadErrorMessage"]);
            }

            var team = await _teamRepository.GetTeamById(project.TeamId.Value);
            if (team == null)
            {
                return redirectToErrorPage(_localizer["teamOverviewLoadErrorTitle"], _localizer["teamOverviewLoadErrorMessage"]);
            }

            var memberships = await _membershipRepository.GetMembershipByTeam(team.Id);
            if (memberships == null)
            {
                return redirectToErrorPage(_localizer["membershipsByUserLoadErrorTitle"], _localizer["membershipsByUserLoadErrorMessage"]);
            }

            var usersInProject = memberships.Select(x => x.UserId).ToList();

            var users = await _applicationUserRepository.GetUsersByIds(usersInProject, organizationCode);

            if (users == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }

            var tasks = await _taskRepository.RetrieveByProject(project.ID.Value, organizationCode);
            if (tasks == null)
            {
                //return redirectToErrorPage(_localizer["tasksByUserLoadErrorTitle"], _localizer["tasksByUserLoadErrorMessage"]);
				return this.RedirectToErrorPage(_localizer["tasksByUserLoadErrorTitle"], _localizer["tasksByUserLoadErrorMessage"]);
			}

            var analysisModel = new AnalysisProjectModel()
            {
                Project = project,
                Team = team,
                Users = users.Values.ToList(),
                Tasks = tasks
            };

            return View(analysisModel);
        }

        [HttpGet]
		public async Task<IActionResult> DownloadFile(Guid id)
		{
			var file = await _organizationFilesRepository.Retrieve(id);

			if (file == null)
            {
				return NotFound();
			}
				
			var fileBytes = file.Content;
			var contentType = string.IsNullOrEmpty(file.ContentType) ? "application/octet-stream" : file.ContentType;
			var fileName = string.IsNullOrEmpty(file.FileName) ? "download.txt" : file.FileName;

			return File(fileBytes, contentType, fileName);
		}

        [HttpPost]
        public async Task<IActionResult> DeletedFile(Guid id)
        {
            var result = await _organizationFilesRepository.Delete(id);

            if (result)
            {
				return Json(new ServerResponse(true) { Message = _localizer["FileDeletedSuccessfully"] });
			}

			return Json(new ServerResponse(false) { Message = _localizer["FailedToDeleteFile"] });
		}

		[HttpGet]
		public async Task<IActionResult> GetFilesHtml(Guid projectId)
		{
			var orgFiles = await _organizationFilesRepository.RetrieveLightProjectFile(projectId);
			var html = await this.RenderViewAsync("_ProjectFileListPartial", orgFiles.ToList());

			return Json(new ServerResponse(true) 
            {  
                Html = html,
				Message = _localizer["YourOperationSuccessful"]
			});
		}

		[RequestSizeLimit(30 * 1024 * 1024)] // 30Mb
		[RequestFormLimits(MultipartBodyLengthLimit = 30 * 1024 * 1024)] // 30Mb
		[HttpPost]
		public async Task<IActionResult> UploadSingleFile(Guid projectId, IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				return Json(new ServerResponse(false) { Message = _localizer["FileMissingOrEmpty"] });
			}

			var organizationCode = this.GetOrganizationCode();

			var convert = new FileConversionRequest()
			{
				File = file,
				ProjectId = projectId,
				OrganizationCode = organizationCode
			};

			var orgFile = await СonverterFiles.ConverterFilesToOrganizationFilesDtoAsync(convert);
			var resultInsert = await _organizationFilesRepository.Insert(orgFile);

			if (resultInsert)
			{
				return Json(new ServerResponse(resultInsert) { Message = _localizer["YourOperationSuccessful"] });
			}
			else
			{
				return Json(new ServerResponse(resultInsert) { Message = _localizer["FileSaveFailed"] });
			}
		}

		private ActionResult redirectToErrorPage(string errorTitle, string errorMsg)
        {
            TempData["ErrorTitle"] = errorTitle;
            TempData["ErrorMessage"] = errorMsg;
            return RedirectToAction("Error", "Project");
        }
        //View error project 
        public ActionResult Error()
        {
            if(TempData["ErrorTitle"] != null)
            {
                ViewBag.ErrorTitle = TempData["ErrorTitle"];
            }
            else
            {
                ViewBag.ErrorTitle = _localizer["PageNotFound"];
            }
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }
    }
}
