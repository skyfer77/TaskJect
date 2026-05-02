using Domain.Database;
using Domain.IServices;
using TaskJect.Web.Models;
using TaskJect.Web.Resources;
using TaskJect.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using System.Text.Json;
using AutoMapper;
using TaskJect.Web.Enums;
using System.Text.RegularExpressions;

namespace TaskJect.Web.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IProjectRepository _projectRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IAvailableProjectPermissionChecker _projectPermissionChecker;
        private readonly IOrganizationStorageChecker _organizationStorageChecker;
        private readonly IOrganizationFilesRepository _organizationFilesRepository;
        private readonly IStringLocalizer<ErrorResources> _localizer;
        private readonly IGitHubService _gitHubService;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IMapper _mapper;
        private readonly ITariffPlanRepository _tariffPlanRepository;
        private readonly ITariffPlanHistoryRepository _tariffPlanHistoryRepository;
		public TaskController(IHttpClientFactory httpClientFactory, IProjectRepository projectRepository,
            ITaskRepository taskRepository, IApplicationUserRepository applicationUserRepository, 
            IMembershipRepository membershipRepository, ITeamRepository teamRepository,
            IAvailableProjectPermissionChecker projectPermissionChecker, IOrganizationStorageChecker organizationStorageChecker,
			IOrganizationFilesRepository organizationFilesRepository, IOrganizationRepository organizationRepository,
			IGitHubService gitHubService,
            IStringLocalizer<ErrorResources> localizer, IMapper mapper , ITariffPlanRepository tariffPlanRepository , 
            ITariffPlanHistoryRepository tariffPlanHistoryRepository)
        {
            _httpClientFactory = httpClientFactory;
            _applicationUserRepository = applicationUserRepository;
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
            _membershipRepository = membershipRepository;
            _teamRepository = teamRepository;
            _projectPermissionChecker = projectPermissionChecker;
            _organizationStorageChecker = organizationStorageChecker;
            _organizationFilesRepository = organizationFilesRepository;
            _organizationRepository = organizationRepository;
            _gitHubService = gitHubService;
            _localizer = localizer;
            _mapper = mapper;
            _tariffPlanHistoryRepository = tariffPlanHistoryRepository;
            _tariffPlanRepository = tariffPlanRepository;
        }

        [Route("{controller=Task}/{action=Index}/{ProjectId}")]
        public async Task<ActionResult> Index(Guid projectId)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var permissionUser = await _projectPermissionChecker.Check(projectId, userId);
            if (permissionUser == null || !permissionUser.CanReadTask)
            {
                return RedirectToAction("PageNotAccess", "Error");
            }

            var project = await _projectRepository.Retrieve(projectId);
            if (project == null)
            {
                return redirectToErrorPage(_localizer["projectOverviewLoadErrorTitle"], _localizer["projectOverviewLoadErrorMessage"]);
            }

            var memberships = await _membershipRepository.GetMembershipByTeam(project.TeamId.Value);
            if (memberships == null)
            {
                return redirectToErrorPage(_localizer["allMembershipsLoadErrorTitle"], _localizer["allMembershipsLoadErrorMessage"]);
            }

            var usersInTeam = memberships.Select(x => x.UserId).ToList();

            var organizationCode = this.GetOrganizationCode();
            var users = await _applicationUserRepository.GetUsersByIds(usersInTeam, organizationCode);

            if (users == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }

            var tasks = await _taskRepository.RetrieveByProject(projectId, organizationCode, false);
            if (tasks == null)
            {
                return redirectToErrorPage(_localizer["tasksByUserLoadErrorTitle"], _localizer["tasksByUserLoadErrorMessage"]);
            }

            foreach (var taskObj in tasks)
            {
                if (taskObj.AssigneeID != null && users.TryGetValue(taskObj.AssigneeID, out var user))
                {
                    taskObj.User = user;
                }
            }

            ViewBag.Permission = permissionUser;
            ViewBag.IsProjectManager = project.ManagerID == userId;

            var task = new TaskPageModel()
            {
                Tasks = _mapper.Map<List<TaskView>>(tasks),
                ProjectId = projectId,
                ProjectName = project.Title,
                Users = users.Values.ToList(),
            };

            return View(task);
        }

        [HttpGet("/Task/TaskListView/{projectId}")]
        public async Task<ActionResult> TaskListView(Guid projectId)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var permissionUser = await _projectPermissionChecker.Check(projectId, userId);
            if (permissionUser == null || !permissionUser.CanReadTask)
            {
                return RedirectToAction("PageNotAccess", "Error");
            }

            var project = await _projectRepository.Retrieve(projectId);
            if (project == null)
            {
                return redirectToErrorPage(_localizer["projectOverviewLoadErrorTitle"], _localizer["projectOverviewLoadErrorMessage"]);
            }
            var memberships = await _membershipRepository.GetMembershipByTeam(project.TeamId.Value);
            if (memberships == null)
            {
                return redirectToErrorPage(_localizer["allMembershipsLoadErrorTitle"], _localizer["allMembershipsLoadErrorMessage"]);
            }

            var usersInTeam = memberships.Select(x => x.UserId).ToList();

            var organizationCode = this.GetOrganizationCode(); ;

            var users = await _applicationUserRepository.GetUsersByIds(usersInTeam, organizationCode);
            if (users == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }

            var tasks = await _taskRepository.RetrieveByProject(projectId, organizationCode);
            if (tasks == null)
            {
                return redirectToErrorPage(_localizer["tasksByUserLoadErrorTitle"], _localizer["tasksByUserLoadErrorMessage"]);
            }

            foreach (var taskObj in tasks)
            {
                if (taskObj.AssigneeID != null && users.TryGetValue(taskObj.AssigneeID, out var user))
                {
                    taskObj.User = user;
                }
            }

            ViewBag.Permission = permissionUser;
            ViewBag.IsProjectManager = project.ManagerID == userId;

            var task = new TaskPageModel()
            {
                Tasks = _mapper.Map<List<TaskView>>(tasks),
                ProjectId = projectId,
                ProjectName = project.Title,
            };

            return View(task);
        }

        [HttpGet]
        public async Task<ActionResult> LoadColumn(TaskStatusView status, TaskPeriodView period, Guid projectId)
        {
			if (period == TaskPeriodView.None)
			{
				return Json(new ServerResponse(true)
				{
					Html = ""
				});
			}

			var userId = this.GetUserId();
            var organizationCode = this.GetOrganizationCode();

            var permissionUser = await _projectPermissionChecker.Check(projectId, userId);
            if (permissionUser == null || !permissionUser.CanReadTask)
            {
                return RedirectToAction("PageNotAccess", "Error");
            }

            var project = await _projectRepository.Retrieve(projectId);
            if (project == null)
            {
                return redirectToErrorPage(_localizer["projectOverviewLoadErrorTitle"], _localizer["projectOverviewLoadErrorMessage"]);
            }
            var memberships = await _membershipRepository.GetMembershipByTeam(project.TeamId.Value);
            if (memberships == null)
            {
                return redirectToErrorPage(_localizer["allMembershipsLoadErrorTitle"], _localizer["allMembershipsLoadErrorMessage"]);
            }

            var usersInTeam = memberships.Select(x => x.UserId).ToList();

            var users = await _applicationUserRepository.GetUsersByIds(usersInTeam, organizationCode);

            if (users == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }

			var startDate = getStartDateByPeriod(period);

			var tasks = await _taskRepository.RetrieveByProject(projectId, organizationCode, startDate);
            if (tasks == null)
            {
                return redirectToErrorPage(_localizer["tasksByUserLoadErrorTitle"], _localizer["tasksByUserLoadErrorMessage"]);
            }

            foreach (var taskObj in tasks)
            {
                if (taskObj.AssigneeID != null && users.TryGetValue(taskObj.AssigneeID, out var user))
                {
                    taskObj.User = user;
                }
            }

            ViewBag.Permission = permissionUser;
            ViewBag.IsProjectManager = project.ManagerID == userId;

            var tasksView = _mapper.Map<List<TaskView>>(tasks);

            var html = await this.RenderViewAsync("_TaskCardList", tasksView);

            return Json(new ServerResponse(true)
            {
                Html = html
            });
        }

		private DateTime? getStartDateByPeriod(TaskPeriodView period)
		{
			var now = DateTime.UtcNow.Date;
			return period switch
			{
				TaskPeriodView.Week => now.AddDays(-7),
				TaskPeriodView.TwoWeeks => now.AddDays(-14),
				TaskPeriodView.Month => now.AddMonths(-1),
				TaskPeriodView.ThreeMonths => now.AddMonths(-3),
				TaskPeriodView.SixMonths => now.AddMonths(-6),
				TaskPeriodView.Year => now.AddYears(-1),
				TaskPeriodView.All => null,
				_ => null,
			};
        }

        [HttpPost]
        public async Task<ActionResult> EditStatus(Guid id, TaskStatusView status)
        {
            var organizationCode = this.GetOrganizationCode();
            var task = await _taskRepository.Retrieve(id, organizationCode);
            if (task == null)
            {
                return redirectToErrorPage(_localizer["singleTaskLoadErrorTitle"], _localizer["singleTaskLoadErrorMessage"]);
            }
            if (task.AssigneeID != null)
            {
                var assingeeUser = await _applicationUserRepository.GetUserById(task.AssigneeID, organizationCode);
                if (assingeeUser != null)
                {
                    task.User = assingeeUser;
                }
            }

            task.Status = _mapper.Map<Domain.Enums.TaskStatus>(status);

			if (status == TaskStatusView.InProgress && string.IsNullOrWhiteSpace(task.GitHubBranch))
			{
				var createBranch = new BranchIssueGitHubParams()
				{
					OrganizationId = Guid.Parse(organizationCode),
					ProjectId = task.ProjectID,
					TaskId = task.ID.Value,
					NewBranchName = createToGitHubBranchName(task.Title)
				};

				var resultCreate = await _gitHubService.CreateBranchGitHub(createBranch);
				if (resultCreate)
				{
					task.GitHubBranch = createBranch.NewBranchName;
				}
			}

			var result = await _taskRepository.Update(task);

			return Json(new ServerResponse(result));
        }

		private string createToGitHubBranchName(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return string.Empty;
			}

			var branchName = value
				.ToLowerInvariant();

			branchName = Regex.Replace(branchName, @"\s+", "-");

			branchName = Regex.Replace(
				branchName,
				@"[^a-z0-9\u0400-\u04FF\-_]",
				string.Empty);

			branchName = Regex.Replace(branchName, @"-+", "-");

			branchName = Regex.Replace(branchName, @"^-+|-+$", string.Empty);

			return branchName;
		}

		[HttpPost]
		public async Task<IActionResult> ArchiveCompleted(Guid projectId)
		{
			try
			{
				var archivedTaskIds = await _taskRepository.ArchiveCompletedTasks(projectId);

				return Json(new ServerResponse(true)
				{
					TaskIds = archivedTaskIds
				});
			}
			catch (Exception)
			{
				return Json(new ServerResponse(false)
				{
					Message = _localizer["ArchiveCompletedFailed"]
				});
			}
		}

		[HttpPost]
        public async Task<ActionResult> CreateTask(Guid projectId, TaskStatusView status)
        {
            var organizationCode = this.GetOrganizationCode();
            var organizationId = Guid.Parse(organizationCode);
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var project = await _projectRepository.Retrieve(projectId);
            if (project == null)
            {
                return redirectToErrorPage(_localizer["projectOverviewLoadErrorTitle"], _localizer["projectOverviewLoadErrorMessage"]);
            }

            var memberships = await _membershipRepository.GetMembershipByTeam(project.TeamId.Value);
            if (memberships == null)
            {
                return redirectToErrorPage(_localizer["allMembershipsLoadErrorTitle"], _localizer["allMembershipsLoadErrorMessage"]);
            }

            var usersInTeam = memberships.Select(x => x.UserId).ToList();

            var users = await _applicationUserRepository.GetUsersByIds(usersInTeam, organizationCode);

            if (users == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }

            var сreator = users
                .Where(u => u.Value.Id == userId)
                .Select(u => $"{u.Value.Name} {u.Value.Surname}")
                .FirstOrDefault();

            ViewBag.IsProjectManager = project.ManagerID == userId;
            var installationId = await _organizationRepository.FindGitHubInstallationId(Guid.Parse(organizationCode));

            var activePlan = await _tariffPlanHistoryRepository.RetrieveActive(organizationId);
            var currentPlan = await _tariffPlanRepository.Retrieve(activePlan.TariffPlanCode);
            ViewBag.HasGitHubIntegration = currentPlan.HasGitHubIntegration;

            var gitHubInegrate = new GitHubViewModel()
            {
                IsGitHubIntegration = installationId != null 
                    && !string.IsNullOrEmpty(project.GitHubOwner) 
                    && !string.IsNullOrEmpty(project.GitHubRepoName),
                IsCreateMode = true,
                CreateNewIssue = true,
                TaskStatus = status
            };
            var taskCreate = new CreateTaskModel()
            {
                ProjectId = projectId,
                Status = status,
                Users = users.Values.ToList(),
                CreatedByUserId = userId,
                Creator = сreator,
                CreatedDate = DateTime.UtcNow,
                GitHubViewModel = gitHubInegrate
            };

            return PartialView("_CreateTask", taskCreate);
        }

        [HttpPost]
        public async Task<JsonResult> Create([FromForm] TaskView task)
        {
            if (task != null)
            {
                var organizationCode = this.GetOrganizationCode();
                var organizationId = Guid.Parse(organizationCode);
                if (!await _organizationStorageChecker.CheckAsync(organizationId))
                {
                    return Json(new ServerResponse(false) { Message = _localizer["StorageYourOrganizationFull"] });
                }
                var userId = this.GetUserId();

                var permissionUser = await _projectPermissionChecker.Check(task.ProjectID, userId);

                if ((permissionUser != null && permissionUser.CanCreateTask) || User.IsInRole("Moderator") || User.IsInRole("Admin")
                    || User.IsInRole("God") || User.IsInRole("TeamLead"))
                {
                    task.ID = Guid.NewGuid();
                    task.StartDate = DateTime.Now;

                    if (task.Status == TaskStatusView.Done)
                    {
                        task.ComplitedDate = DateTime.Now;
                    }

                    if (task.IsGitHubIntegration)
                    {
                        var createBranchIsseu = new BranchIssueGitHubParams()
                        {
                            OrganizationId = organizationId,
                            ProjectId = task.ProjectID,
                            TaskId = task.ID.Value,
                            TaskStatus = task.Status,
                            NewBranchName = task.GitHubBranch,
                            GitHubIssueNumber = task.GitHubIssueNumber,
                            CreateNewIssue = task.CreateNewIssue,
                            TitleTask = task.Title,
                            DescriptionTask = task.Description
                        };

                        var gitHubInfo = await _gitHubService.CreateGitHubBranchAndIssue(createBranchIsseu);
                        if (gitHubInfo != null)
                        {
							task.GitHubBranch = gitHubInfo.BranchName;
							task.GitHubOwner = gitHubInfo.Owner;
                            task.GitHubRepoName = gitHubInfo.RepoName;
                            task.GitHubIssueNumber = gitHubInfo.GitHubIssueNumber;
                        }
                    }
                    else
                    {
                        task.GitHubBranch = null;
                        task.GitHubOwner = null;
                        task.GitHubRepoName = null;
                        task.GitHubIssueNumber = null;
                    }

                    task.OrganizationCode = organizationCode;

                    var result = await _taskRepository.Insert(_mapper.Map<TaskDto>(task));
                    if (result)
                    {

						ViewBag.Permission = permissionUser;
                        if (task != null && task.AssigneeID != null)
                        {
							task.User = await _applicationUserRepository.GetUserById(task.AssigneeID, organizationCode);
						}
						var html = await this.RenderViewAsync("_TaskCard", task);

						return Json(new ServerResponse(result)
						{
							Message = _localizer["TaskSuccessfullyCreated"],
							TaskId = task.ID,
							ProjectId = task.ProjectID,
                            TaskStatus = task.Status,
							Html = html
						});
                    }
                    else
                    {
						return Json(new ServerResponse(result) { Message = _localizer["YourOperationWasNotSuccessful"] });
                    }
                }
            }

            return Json(new ServerResponse(false) { Message = _localizer["YourOperationWasNotSuccessful"] });
        }

        [HttpPost]
        public async Task<ActionResult> DetailsTask(Guid? id, Guid projectId)
        {
            var userId = this.GetUserId();

            var project = await _projectRepository.Retrieve(projectId);
            if(project == null)
            {
                return redirectToErrorPage(_localizer["projectOverviewLoadErrorTitle"], _localizer["projectOverviewLoadErrorMessage"]);
            }

            var memberships = await _membershipRepository.GetMembershipByTeam(project.TeamId.Value);
            if (memberships == null)
            {
                return redirectToErrorPage(_localizer["allMembershipsLoadErrorTitle"], _localizer["allMembershipsLoadErrorMessage"]);
            }

            var usersInTeam = memberships.Select(x => x.UserId).ToList();

            var organizationCode = this.GetOrganizationCode();
            var organizationId = Guid.Parse(organizationCode);
            var users = await _applicationUserRepository.GetUsersByIds(usersInTeam, organizationCode);

            if (users == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }

            var task = await _taskRepository.Retrieve(id.Value, organizationCode);
            if (task == null)
            {
                return redirectToErrorPage(_localizer["singleTaskLoadErrorTitle"], _localizer["singleTaskLoadErrorMessage"]);
            }

            if (task.AssigneeID != null && users.TryGetValue(task.AssigneeID, out var user))
            {
                task.User = user;
            }

            var activePlan = await _tariffPlanHistoryRepository.RetrieveActive(organizationId);
            var currentPlan = await _tariffPlanRepository.Retrieve(activePlan.TariffPlanCode);
            ViewBag.HasGitHubIntegration = currentPlan.HasGitHubIntegration;
            var taskFiles = await _organizationFilesRepository.RetrieveLightTaskFile(id.Value);

            var permissionUser = await _projectPermissionChecker.Check(projectId, userId);
            if (permissionUser == null || !permissionUser.CanReadTask)
            {
                return RedirectToAction("PageNotAccess", "Error");
            }

            var сreator = users
                .Where(u => u.Value.Id == task?.CreatedByUserId)
                .Select(u => $"{u.Value.Name} {u.Value.Surname}")
                .FirstOrDefault();

            ViewBag.IsProjectManager = project.ManagerID == userId;
            var installationId = await _organizationRepository.FindGitHubInstallationId(Guid.Parse(organizationCode));

            var gitHubInegrate = new GitHubViewModel()
            {
				IsGitHubIntegration = installationId != null
					&& !string.IsNullOrEmpty(project.GitHubOwner)
					&& !string.IsNullOrEmpty(project.GitHubRepoName),
				Branch = task.GitHubBranch,
                Issue = task.GitHubIssueNumber,
                CreateNewIssue = false,
                IsCreateMode = false,
                TaskStatus = (TaskStatusView)task.Status
            };
            var model = new DetailsTaskViewModel
            {
	            TaskEditModel = new EditTaskModel
	            {
		            Task = _mapper.Map<TaskView>(task),
		            Users = users.Values.ToList(),
		            Creator = сreator,
                    OrganizationFiles = taskFiles.ToList(),
				},
	            Permissions = permissionUser,
                GitHubViewModel = gitHubInegrate
            };

            return PartialView("_DetailsTask", model);
        }

        [HttpPost]
        public async Task<JsonResult> Edit(TaskView task, string filesToDelete, int? linkGitHubIssueNumber)
        {
            if (task != null)
            {
                var organizationCode = this.GetOrganizationCode();
                var organizationId = Guid.Parse(organizationCode);
                if (!await _organizationStorageChecker.CheckAsync(organizationId))
                {
                    return Json(new ServerResponse(false) { Message = _localizer["StorageYourOrganizationFull"] });
                }
                task.OrganizationCode = organizationCode;
                task.DateEdit = DateTime.Now;

				if (task.IsGitHubIntegration)
				{
                    var branchIssueParams = new BranchIssueGitHubParams()
                    {
                        OrganizationId = organizationId,
                        ProjectId = task.ProjectID,
                        TaskId = task.ID.Value,
                    };

					var hasBranch = task.ID.HasValue && await _taskRepository.BranchExistsForTask(task.ID.Value);
					if (task.Status == TaskStatusView.InProgress && !hasBranch)
					{
                        branchIssueParams.NewBranchName = createToGitHubBranchName(task.Title);

						var resultCreate = await _gitHubService.CreateBranchGitHub(branchIssueParams);
						if (resultCreate)
						{
							task.GitHubBranch = branchIssueParams.NewBranchName;
						}
					}

					if (!task.GitHubIssueNumber.HasValue && linkGitHubIssueNumber.HasValue)
					{
                        branchIssueParams.GitHubIssueNumber = linkGitHubIssueNumber;

						var resultCheck = await _gitHubService.LinkGitHubIssue(branchIssueParams);
						if (resultCheck)
						{
							task.GitHubIssueNumber = linkGitHubIssueNumber;
						}
					}
				}

                var result = await _taskRepository.Update(_mapper.Map<TaskDto>(task));
                if (result)
                {
					var idsfilesToDelete = parseGuidsFromJson(filesToDelete);

                    if (idsfilesToDelete != null && idsfilesToDelete.Count != 0)
                    {
                        await _organizationFilesRepository.DeleteFiles(idsfilesToDelete);
					}

                    var userId = this.GetUserId();
					var permissionUser = await _projectPermissionChecker.Check(task.ProjectID, userId);
					ViewBag.Permission = permissionUser;
					if (task != null && task.AssigneeID != null)
					{
						task.User = await _applicationUserRepository.GetUserById(task.AssigneeID, organizationCode);
					}
					
                    if (task == null)
                    {
						return Json(new ServerResponse(false) { Message = _localizer["YourOperationWasNotSuccessful"] });
					}

					var html = await this.RenderViewAsync("_TaskCard", task);

					return Json(new ServerResponse(result) { 
                        Message = _localizer["TaskSuccessfullyChanged"],
						TaskStatus = task.Status,
						Html = html
                    });
                }
                else
                {
                    return Json(new ServerResponse(result) { Message = _localizer["YourOperationWasNotSuccessful"] });
                }
            }
            else
            {
                return Json(new ServerResponse(false) { Message = _localizer["TaskModelEqualsNull"] });
            }
        }

		[HttpPost]
		[RequestSizeLimit(30 * 1024 * 1024)] // 30Mb
		[RequestFormLimits(MultipartBodyLengthLimit = 30 * 1024 * 1024)] // 30Mb
		public async Task<IActionResult> UploadSingleFile(Guid taskId, Guid projectId, IFormFile file)
		{
			if (file == null || file.Length == 0)
            {
				return Json(new ServerResponse(false) { Message = _localizer["FileMissingOrEmpty"] });
			}

			var organizationCode = this.GetOrganizationCode();

			if (!await _organizationStorageChecker.CheckAsync(Guid.Parse(organizationCode)))
			{
				return Json(new ServerResponse(false) { Message = _localizer["StorageYourOrganizationFull"] });
			}

			var convert = new FileConversionRequest()
			{
				File = file,
				TaskId = taskId,
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
        public async Task<JsonResult> Delete(Guid id)
        {
            if (id != Guid.Empty)
            {
				await _organizationFilesRepository.DeleteByTaskId(id);

				var result = await _taskRepository.Delete(id);
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
                return Json(new ServerResponse(false) { Message = _localizer["IdTaskEqualsNull"] });
            }
        }

        [HttpPost]
        public async Task<JsonResult> AddHours(AddHoursModel model)
        {
            if (!string.IsNullOrEmpty(model.TaskId) && (model.NewHours.HasValue || model.NewMinutes.HasValue))
            {
                var task = await setNewHorseMinutsTask(model);

                var result = await _taskRepository.Update(task);
                if (result)
                {
                    return Json(new ServerResponse(result) { 
                        Message = _localizer["YourOperationSuccessful"],
                        TaskId = task.ID,
                        Html = getActualTimeDisplay(task),
					});
                }
                else
                {
                    return Json(new ServerResponse(result) { Message = _localizer["YourOperationWasNotSuccessful"] });
                }
            }
            else
            {
                return Json(new ServerResponse(false) { Message = _localizer["TaskIdHoursMinutesNull"] });
            }
        }

        private string getActualTimeDisplay(TaskDto task)
        {
            var hours = task.ActualHours;
            var minutes = task.ActualMinutes;

            if (hours == null && minutes == null)
            {
				return string.Empty;
			}
                
            var result =
                (hours != null ? $"{hours}{_localizer["h"]} " : "") +
                (minutes != null ? $"{minutes}{_localizer["m"]}" : "");

            return result.Trim();
        }

        //Повертаємо файл для скачування
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

		private async Task<TaskDto> setNewHorseMinutsTask(AddHoursModel model)
        {
            var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;
            var task = await _taskRepository.Retrieve(Guid.Parse(model.TaskId), organizationCode);

            if (task != null)
            {
                task.ActualHours = task.ActualHours ?? 0;
                task.ActualMinutes = task.ActualMinutes ?? 0;

                if (model.NewHours.HasValue)
                {
                    task.ActualHours += model.NewHours;
                }

                if (model.NewMinutes.HasValue)
                {
                    var totalMinutes = task.ActualMinutes + model.NewMinutes;

                    if (totalMinutes >= 60)
                    {
                        task.ActualHours += totalMinutes / 60;
                        task.ActualMinutes = totalMinutes % 60;
                    }
                    else
                    {
                        task.ActualMinutes = totalMinutes;
                    }

                    if (task.ActualMinutes == 0)
                    {
                        task.ActualMinutes = null;
                    }
                }
            }
            else if (task == null)
            {
                TempData["ErrorTitle"] = _localizer["singleTaskLoadErrorTitle"];
                TempData["ErrorMessage"] = _localizer["singleTaskLoadErrorMessage"];
                RedirectToAction("Error", "Task");
            }

            return task;
        }

		private ActionResult redirectToErrorPage(string errorTitle, string errorMsg)
        {
            TempData["ErrorTitle"] = errorTitle;
            TempData["ErrorMessage"] = errorMsg;
            return RedirectToAction("Error", "Task");
        }

        //Error view task
        public IActionResult Error()
        {
            if (TempData["ErrorTitle"] != null)
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
