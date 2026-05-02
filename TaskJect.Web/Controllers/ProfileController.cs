using TaskJect.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TaskJect.Web.Services;
using Domain.Database;
using Domain.IServices;
using Data;
using TaskJect.Web.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Domain.DomainEvents;
using Data.DomainEvent;

namespace TaskJect.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IEmailSender _emailSender;
        private readonly ITeamRepository _teamRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IStringLocalizer<ErrorResources> _localizer;
        private readonly ILogger<ProfileController> _logger;
        private readonly ITelegramLinkBuilder _telegramLinkBuilder;
        private readonly ITelegramTicketGenerator _ticketGenerator;
		private readonly IOrganizationFilesRepository _organizationFilesRepository;
        private readonly ITariffPlanRepository _tariffPlanRepository;
        private readonly ITariffPlanHistoryRepository _tariffPlanHistoryRepository;
        private readonly IMapper _mapper;

		private readonly DomainEventDispatcher _dispatcher;
		public ProfileController(IHttpClientFactory httpClientFactory, IProjectRepository projectRepository, 
            ITaskRepository taskRepository, IApplicationUserRepository applicationUserRepository,
            IEmailSender emailSender, ITeamRepository teamRepository, IMembershipRepository membershipRepository,
            IStringLocalizer<ErrorResources> localizer, ILogger<ProfileController> logger,
            ITelegramLinkBuilder telegramLinkBuilder, ITelegramTicketGenerator telegramTicketGenerator, 
            IOrganizationFilesRepository organizationFilesRepository, IOrganizationRepository organizationRepository, 
            ITariffPlanRepository tariffPlanRepository, ITariffPlanHistoryRepository tariffPlanHistoryRepository,
        IMapper mapper, DomainEventDispatcher dispatcher)
        {
            _httpClientFactory = httpClientFactory;
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
            _applicationUserRepository = applicationUserRepository;
            _emailSender = emailSender;
            _teamRepository = teamRepository;
            _membershipRepository = membershipRepository;
            _localizer = localizer;
            _logger = logger;
            _telegramLinkBuilder = telegramLinkBuilder;
            _ticketGenerator = telegramTicketGenerator;
            _organizationFilesRepository = organizationFilesRepository;
            _organizationRepository = organizationRepository;
            _tariffPlanRepository = tariffPlanRepository;
            _tariffPlanHistoryRepository = tariffPlanHistoryRepository;
            _mapper = mapper;

            _dispatcher = dispatcher;
        }

        [Authorize]
        public async Task<ActionResult> Index(string id = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id != null)
            {
                userId = id;
            }

            var organizationCode = this.GetOrganizationCode();
            if (string.IsNullOrEmpty(organizationCode))
            {
                return RedirectToAction("Logout", "Account");
            }
            var user = await _applicationUserRepository.GetUserById(userId, organizationCode);

            if (user == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }

            if (string.IsNullOrEmpty(user.TelegramTicket))
            {
                user.TelegramTicket = _ticketGenerator.GenerateTicket();
                await _applicationUserRepository.UpdateUser(user, organizationCode);
            }

            var telegramLink = _telegramLinkBuilder.BuildLink(user.TelegramTicket);

            var memberships = await _membershipRepository.GetMembershipsByUser(userId);
            if (memberships == null)
            {
                return redirectToErrorPage(_localizer["membershipsByUserLoadErrorTitle"], _localizer["membershipsByUserLoadErrorMessage"]);
            }

            List<Guid> teamIds = new List<Guid>();
            var projectsByTeam = new Dictionary<Guid, List<ProjectDto>>();
            var projects = new List<ProjectDto>();
            foreach (var member in memberships)
            {
                teamIds.Add(member.TeamId);
            }

            if (teamIds.Count() > 0)
            {
                projectsByTeam = await _projectRepository.RetrieveByTeamsIDs(teamIds);

                var teams = await _teamRepository.GetTeamByIds(teamIds.Distinct());

                if (projectsByTeam != null)
                {
                    projects = projectsByTeam.Values.SelectMany(x => x).ToList();

                    foreach (var project in projects)
                    {
                        if (project.TeamId != null && teams.TryGetValue(project.TeamId.Value, out var team))
                        {
                            project.Team = team;
                        }
                    }
                }
                else if (projectsByTeam == null)
                {
                    return redirectToErrorPage(_localizer["projectsByOrganizationLoadErrorTitle"], _localizer["projectsByOrganizationLoadErrorMessage"]);
                }
            }

            var tasks = await _taskRepository.RetriveByUser(userId, organizationCode, 10);

            if (tasks == null)
            {
                return redirectToErrorPage(_localizer["tasksByUserLoadErrorTitle"], _localizer["tasksByUserLoadErrorMessage"]);
            }
            //TODO: rework added task files
            var usingProjectIds = tasks.Select(t => t.ProjectID).Distinct().ToList();
            var projectsDict = (await _projectRepository.RetrieveByProjectIDs(usingProjectIds)).ToDictionary(k => k.ID, v => v.Title);
            var taskProjectDict = tasks.ToDictionary(t => t, t => projectsDict.ContainsKey(t.ProjectID) ? projectsDict[t.ProjectID] : "Unknown");

            var profile = new ProfileViewModel
            {
                User = user,
                Projects = projects,
                TasksWithProjectNames = taskProjectDict,
                ThisUserId = userId,
                PersonalTelegramLink = telegramLink,
            };

            return View(profile);
        }

		[HttpGet]
        public async Task<ActionResult> ColleaguesBirthdays()
        {
            var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;

            var currentMonth = DateTime.Now.Month;

            var users = await _applicationUserRepository.GetAllUsersTheOrganization(organizationCode);

            if (users == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }

            var startBirthdayTrackingDay = DateTime.Today;
            var endBirthdayTrackingDay = DateTime.Today.AddMonths(1);
            var usersWithBirthdays = users.Where(u =>
                {
                    if (u.Birthday.HasValue)
                    {
                        var birthdayThisYear = new DateTime(startBirthdayTrackingDay.Year, u.Birthday.Value.Month, u.Birthday.Value.Day);
                        return birthdayThisYear >= startBirthdayTrackingDay && birthdayThisYear <= endBirthdayTrackingDay;
                    }
                    return false;
                }).ToList();

            return PartialView("_ColleaguesBirthdays", usersWithBirthdays);
        }
        [HttpPost]
        public async Task<ActionResult> UnconnectTelegram(string id)
        {
            var result = await _applicationUserRepository.UnconnectTelegramFromUser(id);
            if(result)
            {
                return Json(new ServerResponse(true) { Message = _localizer["UnconnectSuccessful"] });
            }
            else
            {
                return Json(new ServerResponse(false) { Message = _localizer["UnconnectFailed"] });
            }
        }
        [HttpPost]
        public async Task<ActionResult> OverviewTask(Guid id, string userId)
        {
            var organizationCode = this.GetOrganizationCode();
            var organizationId = Guid.Parse(organizationCode);
            var task = await _taskRepository.Retrieve(id, organizationCode);
            if (task == null)
            {
                return redirectToErrorPage(_localizer["tasksByUserLoadErrorTitle"], _localizer["tasksByUserLoadErrorMessage"]);
            }
            var user = await _applicationUserRepository.GetUserById(userId, organizationCode);

            if (user == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }
            task.User = user;
            var activePlan = await _tariffPlanHistoryRepository.RetrieveActive(organizationId);
            var currentPlan = await _tariffPlanRepository.Retrieve(activePlan.TariffPlanCode);
            ViewBag.HasGitHubIntegration = currentPlan.HasGitHubIntegration;
            var taskFiles = await _organizationFilesRepository.RetrieveLightTaskFile(id);
            var installationId = await _organizationRepository.FindGitHubInstallationId(Guid.Parse(organizationCode));
            var overviewTask = new OverviewTaskModel() 
            { 
                Task = _mapper.Map<TaskView>(task),
                OrganizationFiles = taskFiles.ToList(),
                GitHubIntegration = installationId != null
            };

            return PartialView("_OverviewTask", overviewTask);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string Id)
        {
            var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;
            var user = await _applicationUserRepository.GetUserById(Id, organizationCode);

            if (user == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }

            return PartialView("_EditProfile", user);
        }

        [HttpPost]
        public async Task<JsonResult> EditProfile(ApplicationUserLiteView user)
        {
            if (user != null)
            {
                var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;

                var isSuccessUpdate = await _applicationUserRepository.UpdateUser(_mapper.Map<ApplicationUserLiteDto>(user), organizationCode);

                
                if (!isSuccessUpdate)
                {
                    TempData["ErrorTitle"] = _localizer["userUpdateErrorTitle"];
                    TempData["ErrorMessage"] = _localizer["userUpdateErrorMessage"];
                    return Json(new { success = 0 });
                }
                else
                {
                    return Json(new ServerResponse(true) { Message = "Update is success!" });
                }
            }
            else
            {
                return Json(new { Message = "Profile model equals null!" });
            }
        }

		[HttpGet]
		public async Task<JsonResult> DeleteAccountInformation()
		{
			var userId = this.GetUserId();
			var organizationCode = this.GetOrganizationCode();

			if (userId != null)
			{
                var user = await _applicationUserRepository.GetUserById(userId, organizationCode);
				if (user.Role == SD.TeamLead)
				{
					var allUsers = await _applicationUserRepository.GetAllUsersTheOrganization(organizationCode);
                    var otherUsers = allUsers.Where(u => u.Id != userId).ToList();
					if (otherUsers.Any())
					{
                        var html = await this.RenderViewAsync("_SwitchUserRoleModalWindow", otherUsers);
						return Json(new ServerResponse(false) 
                        { 
                            Html = html
                        });
					}
					else
					{
						var html = await this.RenderViewAsync("_OrganizationDeleteModalWindow");
						return Json(new ServerResponse(false)
						{
                            Html = html
						});
					}
				}

                var isSuccess = await _applicationUserRepository.DeleteUser(userId);
				if (isSuccess)
				{
					await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
					return Json(new ServerResponse(isSuccess) { RedirectUrl = Url.Action("Index", "Home") });
				}
				else
				{
					return Json(new ServerResponse(isSuccess) { Message = _localizer["DeleteUserWasNotSuccessful"] });
				}
			}
			else
			{
				return Json(new ServerResponse(false) { Message = _localizer["IdUserEqualsNull"] });
			}
		}

		private ActionResult redirectToErrorPage(string errorTitle, string errorMsg)
        {
            TempData["ErrorTitle"] = errorTitle;
            TempData["ErrorMessage"] = errorMsg;
            return RedirectToAction("Error", "Profile");
        }

        //Error view profile
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
