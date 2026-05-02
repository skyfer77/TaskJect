using TaskJect.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Domain.Database;
using TaskJect.Web.Resources;
using Microsoft.Extensions.Localization;
using TaskJect.Web.Services;

namespace TaskJect.Web.Controllers
{
    [Authorize]
    public class TeamController : Controller
    {
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMembershipRepository _membershipRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ITariffPlanHistoryRepository _tariffPlanHistoryRepository;
        private readonly ITariffPlanRepository _tariffPlanRepository;
        private readonly IProjectUserPermissionRepository _projectUserPermissionRepository;
        private readonly IStringLocalizer<ErrorResources> _localizer;
        private readonly IUserCreator _userCreator;
        public TeamController(IHttpClientFactory httpClientFactory, IApplicationUserRepository applicationUserRepository, 
            IMembershipRepository membershipRepository, ITeamRepository teamRepository , ITaskRepository taskRepository , IProjectRepository projectRepository,
            ITariffPlanHistoryRepository tariffPlanHistoryRepository, ITariffPlanRepository tariffPlanRepository,
            IProjectUserPermissionRepository projectUserPermissionRepository,IStringLocalizer<ErrorResources> localizer, IUserCreator userCreator)
        {
            _httpClientFactory = httpClientFactory;
            _applicationUserRepository = applicationUserRepository;
            _membershipRepository = membershipRepository;
            _teamRepository = teamRepository;
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _tariffPlanHistoryRepository = tariffPlanHistoryRepository;
            _tariffPlanRepository = tariffPlanRepository;
            _projectUserPermissionRepository = projectUserPermissionRepository;
            _localizer = localizer;
            _userCreator = userCreator;
        }

        public async Task<IActionResult> Index()
        {
            var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;

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
            if(teams == null)
            {
                return redirectToErrorPage(_localizer["teamsByOrganizationLoadErrorTitle"], _localizer["teamsByOrganizationLoadErrorMessage"]);
            }
            var usersWithCompletedTasks = _taskRepository.RetrieveCountCompletedTasksByUsers(organizationCode);

            var teamsWithMembers = Users(users, memberships, teams);
            var currentTariff = await _tariffPlanHistoryRepository.RetrieveActive(Guid.Parse(organizationCode));
            var tariff = await _tariffPlanRepository.Retrieve(currentTariff.TariffPlanCode);
            ViewBag.maxUsers = tariff.MaxUsers;
            var teamPageModel = new TeamPageModel
            {
                UsersWithCompletedTasks = usersWithCompletedTasks.Result,
                Users = users,
                Memberships = memberships,
                Teams = teams,
                TeamsWithUsers = teamsWithMembers
            };  
            return View(teamPageModel);
        }

        [Authorize(Roles = "TeamLead, Moderator, God, Admin")]
        [HttpPost]
        public async Task<JsonResult> CreateTeam(TeamDto team)
        {
            if (team != null)
            {
                var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;
                team.OrganizationCode = organizationCode;

                var result = await _teamRepository.Modify(team);

                if (result == null)
                {
                    return Json(new ServerResponse(false) { Message = _localizer["YourOperationWasNotSuccessful"] });
                }
                else
                {
                    return Json(new ServerResponse(true) { Message = _localizer["YourOperationSuccessful"] });
                }
            }
            else
            {
                return Json(new ServerResponse(false) { Message = _localizer["YourOperationSuccessful"] });
            }
        }

        [Authorize(Roles = "TeamLead, Moderator, God, Admin")]
        [HttpPost]
        public async Task<JsonResult> ManageTeam(Guid IdTeam, string[] Ids , string newTeamName)
        {
            if (Ids == null || Ids.Length == 0)
            {
                return Json(new ServerResponse(false) { Message = _localizer["TeamModelQualsNull"] });
            }

            var team = await _teamRepository.GetTeamById(IdTeam);
            if (team == null)
            {
                return Json(new ServerResponse(false) { Message = _localizer["TeamNotFound"] });
            }

            if (!string.IsNullOrWhiteSpace(newTeamName))
            {
                var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;
                team.OrganizationCode = organizationCode;
                team.Name = newTeamName;

                var updateResult = await _teamRepository.Modify(team);
                if (updateResult == null)
                {
                    return Json(new ServerResponse(false) { Message = _localizer["TeamNameUpdateWasNotSuccessful"] });
                }
            }

            var memberships = await _membershipRepository.GetMemberships();
            if (memberships == null)
            {
                redirectToErrorPage(_localizer["allMembershipsLoadErrorTitle"], _localizer["allMembershipsLoadErrorMessage"]);
            }

            var usersToAdd = Ids.Where(userId => !memberships.Any(m => m.UserId == userId && m.TeamId == IdTeam)).ToList();
            var usersToDelete = memberships
                .Where(m => m.TeamId == IdTeam && !Ids.Contains(m.UserId))
                .Select(m => m.UserId)
                .Distinct()
                .ToList();

            if (usersToAdd.Count == 0 && usersToDelete.Count == 0)
            {
                return Json(new ServerResponse(false) { Message = _localizer["ThisMembersAlreadyTeam"] });
            }

            if (usersToDelete.Count != 0)
            {
                var deleteMemberships = new TeamWithTeamMembersSelectDto
                {
                    TeamId = IdTeam,
                    SelectedUsersId = usersToDelete.ToArray()
                };

                await _membershipRepository.Delete(deleteMemberships);
            }

            var newMemberships = new TeamWithTeamMembersSelectDto
            {
                TeamId = IdTeam,
                SelectedUsersId = usersToAdd.ToArray()
            };

            var addResult = await _membershipRepository.Add(newMemberships);
            if (!addResult)
            {
                return Json(new ServerResponse(false) { Message = _localizer["YourOperationWasNotSuccessful"] });
            }

            await updateProjectPermissionsForUsers(IdTeam, usersToAdd);

            return Json(new ServerResponse(true) { Message = _localizer["YourOperationSuccessful"] });
        }

        private async Task<bool> updateProjectPermissionsForUsers(Guid teamId, List<string> Ids)
        {
            var projectIds = await _projectRepository.RetrieveProjectIdsByTeam(teamId);
            if (projectIds == null)
            {
                return false;
            }

            return await _projectUserPermissionRepository.InsertDefaultProjectsPermissionsForUsers(Ids, projectIds.ToArray());
        } 

        [Authorize(Roles = "TeamLead, Moderator, God, Admin")]
        [HttpPost]
        public async Task<IActionResult> EditTeam(Guid Id)
        {
            var team = await _teamRepository.GetTeamById(Id);

            var memberships = await _membershipRepository.GetMemberships();
            if (memberships == null)
            {
                return redirectToErrorPage(_localizer["allMembershipsLoadErrorTitle"], _localizer["allMembershipsLoadErrorMessage"]);
            }

            var membershipsTeam = new List<MembershipDto>();
            foreach (var item in memberships)
            {
                if (item.TeamId.Equals(Id))
                {
                    membershipsTeam.Add(item);
                }
            }

            var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;
            var users = await _applicationUserRepository.GetAllUsersTheOrganization(organizationCode);

            if (users == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }

            var editTeam = new ManageTeamModel
            {
                IdTeam = Id,
                Name = team.Name,
                OrganizationCode = organizationCode,
                Membership = membershipsTeam,
                User = users
            };

            if (team == null)
            {
                return redirectToErrorPage(_localizer["teamOverviewLoadErrorTitle"], _localizer["teamOverviewLoadErrorMessage"]);
            }

            return PartialView("_EditTeam", editTeam);
        }

        [Authorize(Roles = "TeamLead, Moderator, God, Admin")]
        [HttpPost]
        public async Task<JsonResult> DeleteTeam(Guid Id)
        {
            var teamProjects = await _projectRepository.RetrieveProjectsByTeam(Id);
            if (teamProjects == null)
            {
                if (Id != Guid.Empty)
                {
                    var result = await _membershipRepository.DeleteMembersByTeamId(Id);
                    if (result)
                    {
                        var resultTeam = await _teamRepository.Delete(Id);

                        if (resultTeam)
                        {
                            return Json(new ServerResponse(resultTeam) { Message = _localizer["YourOperationSuccessful"] });
                        }
                        else
                        {
                            return Json(new ServerResponse(resultTeam) { Message = _localizer["YourOperationWasNotSuccessful"] });
                        }
                    }
                    else
                    {
                        return Json(new ServerResponse(result) { Message = _localizer["YourOperationWasNotSuccessful"] });
                    }
                }
                else
                {
                    return Json(new ServerResponse(false) { Message = _localizer["IdTeamEqualsNull"] });
                }
            }
            else
            {
                string projectNames = string.Join("<br>", teamProjects.Select((p, index) => $"{index + 1}. {p.Title}"));

                return Json(new ServerResponse(false)
                {
                    Message =  $"<div style='width:800px; margin: auto;text-align: justify;'> {_localizer["TheTeamBeingUsedFollowingProjects"]}:<br><br>" + $"{projectNames}<br><br>" +
                              "Replace the team in these projects before deleting.</div>"
                });
            }
        }

        [Authorize(Roles = "TeamLead, Moderator, God, Admin")]
        [HttpPost]
        public async Task<JsonResult> DeleteOnTeam(string Id, Guid IdTeam)
        {
            if (string.IsNullOrEmpty(Id) || IdTeam == Guid.Empty)
            {
                return Json(new ServerResponse(false) { Message = _localizer["IdTeamIdUserEqualsNull"] });
            }

            var result = await _membershipRepository.DeleteByUserAndTeam(Id, IdTeam);
            if (!result)
            {
                return Json(new ServerResponse(false) { Message = _localizer["YourOperationWasNotSuccessful"] });
            }

            return Json(new ServerResponse(result) { Message = _localizer["YourOperationSuccessful"] });
        }
   
        [Authorize(Roles = "TeamLead, Moderator, God, Admin")]
        [HttpPost]
        public async Task<JsonResult> CreateUser(CreateUserModel model)
        {
            if (!ModelState.IsValid || model == null)
            {
                return Json(new ServerResponse(false) { Message = _localizer["InvalidModelNullModel"] });
            }

            var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;
            
            if (!string.IsNullOrEmpty(organizationCode))
            {
                var newUser = new CreateUserByEmailModel()
                {
                    OrganizationCode = organizationCode,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    Surname = model.Surname,
                    RoleUser = "User"
                };
                var isSuccess = await _userCreator.CreateUser(newUser);

                if (isSuccess)
                {
                    return Json(new ServerResponse(isSuccess) { Message = _localizer["CreateNewUserWithEmailSuccessful"] });
                }
                else
                {
                    return Json(new ServerResponse(isSuccess) { Message = _localizer["CreateNewUserWithEmailWasNotSuccessful"] });
                }
            }
            else
            {
                return Json(new ServerResponse(false) { Message = _localizer["IdOrganizationEmpty"] });
            }
        }

        private ActionResult redirectToErrorPage(string errorTitle, string errorMsg)
        {
            TempData["ErrorTitle"] = errorTitle;
            TempData["ErrorMessage"] = errorMsg;
            return RedirectToAction("Error", "Team");
        }

        //Error view team
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

        private Dictionary<TeamDto, List<ApplicationUserLiteDto>> Users(List<ApplicationUserLiteDto> users, IEnumerable<MembershipDto> memberships, IEnumerable<TeamDto> teams)
        {
            var usersSet = new HashSet<string>(users.Select(x => x.Id));
            var teamsWithUsers = new Dictionary<TeamDto, List<ApplicationUserLiteDto>>();

            foreach (var team in teams)
            {
                var members = memberships.Where(membership => membership.TeamId == team.Id && usersSet.Contains(membership.UserId)).Select(membership => users.First(x => x.Id == membership.UserId)).ToList();
                
                teamsWithUsers.Add(team, members);
            }

            return teamsWithUsers;
        }
    }
}
