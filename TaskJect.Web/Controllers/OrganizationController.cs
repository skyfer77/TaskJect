using Domain.Database;
using TaskJect.Web.DbContexts;
using Domain.Enums;
using TaskJect.Web.Models;
using TaskJect.Web.Resources;
using TaskJect.Web.Services;
using Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using AutoMapper;

namespace TaskJect.Web.Controllers
{
    [Authorize]
    public class OrganizationController : Controller
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly ITariffPlanRepository _tariffPlanRepository;
        private readonly IOrganizationAppealRepository _organizationAppealRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly ITariffPlanHistoryRepository _tariffPlanHistoryRepository;
        private readonly ApplicationDbContext _context;
        private readonly IStringLocalizer<ErrorResources> _localizer;
        private readonly IUserCreator _userCreator;
        private readonly IGumroadLinkProvider _gumroadLinkProvider;
        private readonly IMapper _mapper;
        private readonly ILogger<OrganizationController> _logger;

        public OrganizationController(IOrganizationRepository organizationRepository,
            IApplicationUserRepository applicationUserRepository,
            ITariffPlanRepository tariffPlanRepository, IOrganizationAppealRepository organizationAppealRepository,
            IMembershipRepository membershipRepository, ITaskRepository taskRepository, ITariffPlanHistoryRepository tariffPlanHistoryRepository,
            ApplicationDbContext context, IStringLocalizer<ErrorResources> localizer,
            IGumroadLinkProvider gumroadLinkProvider, IUserCreator userCreator,
            IMapper mapper, ILogger<OrganizationController> logger) 
        {
            _organizationRepository = organizationRepository;
            _applicationUserRepository = applicationUserRepository;
            _tariffPlanRepository = tariffPlanRepository;
            _organizationAppealRepository = organizationAppealRepository;
            _membershipRepository = membershipRepository;
            _taskRepository = taskRepository;
            _tariffPlanHistoryRepository = tariffPlanHistoryRepository;
            _context = context;
            _localizer = localizer;
            _userCreator = userCreator;
            _gumroadLinkProvider = gumroadLinkProvider;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
			var organizationCode = this.GetOrganizationCode();
            var organizationId = Guid.Parse(organizationCode);


			var organization = await _organizationRepository.GetOrganizationById(organizationId);
            if (organization == null)
            {
                return redirectToErrorPage(_localizer["organizationLoadErrorTitle"], _localizer["organizationLoadErrorMessage"]);
            }

            var users = await _applicationUserRepository.GetAllUsersTheOrganization(organizationCode);

            if (users == null)
            {
                return redirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
            }
            var currentTariffPlanHistory = await _tariffPlanHistoryRepository.RetrieveActive(organizationId);
            var tariffPlan = await _tariffPlanRepository.Retrieve(currentTariffPlanHistory.TariffPlanCode);
            var totalAppeal = await _organizationAppealRepository.RetrieveCountThisMonth(organizationId);
            ViewBag.HasGitHubIntegration = tariffPlan.HasGitHubIntegration;
            var organizationInfo = new OrganizationInfo
            {
                Organization = organization,
                Users = _mapper.Map<List<ApplicationUserLiteView>>(users),
                TariffPlan = tariffPlan,
                IsAppealCount = totalAppeal,
                CurrentTariffPlan = currentTariffPlanHistory
            };

            return View(organizationInfo);
        }

		[HttpPost]
		[AllowAnonymous]
		[Route("Organization/Index")]
		public IActionResult PaymentReturn([FromForm] IFormCollection form)
		{
			if (form == null || form.Count == 0)
			{
				_logger.LogWarning("PaymentReturn: received an empty form from WayForPay");
				return Redirect("/Organization?payment=fail");
			}

			if (!form.ContainsKey("transactionStatus") || !form.ContainsKey("reason"))
			{
				_logger.LogWarning("PaymentReturn: There are no required fields in the WayForPay response. Data: {Form}", form);
				return Redirect("/Organization?payment=fail");
			}

			var transactionStatus = form["transactionStatus"].ToString();
			var reason = form["reason"].ToString();

			if (transactionStatus == "Approved")
			{
				return Redirect("/Organization?payment=success");
			}
			else
			{
				return Redirect($"/Organization?payment=fail&reason={reason}");
			}
		}

		[HttpPost]
        public async Task<IActionResult> Edit(Guid Id)
        {
            var organization = await _organizationRepository.GetOrganizationById(Id);

            if (organization == null)
            {
                return redirectToErrorPage(_localizer["organizationLoadErrorTitle"], _localizer["organizationLoadErrorMessage"]);
            }

            return PartialView("_EditOrganization", organization);
        }

        [HttpPost]
        public async Task<JsonResult> EditOrganization(OrganizationDto organization)
        {
            if (!string.IsNullOrEmpty(organization.Name) && organization.OrganizationId != Guid.Empty)
            {
                var newOrganization = await setNewOrganization(organization);
                if (newOrganization != null)
                {
                    var result = await _organizationRepository.Update(newOrganization);

                    if (result)
                    {
                        return Json(new ServerResponse(result) { Message = _localizer["YourOperationSuccessful"] });
                    }
                    else
                    {
                        return Json(new ServerResponse(result) { Message = _localizer["YourOperationWasNotSuccessful"] });
                    }
                }
            }

            return Json(new ServerResponse(false) { Message = _localizer["YourOperationWasNotSuccessful"] });
        }

        private async Task<OrganizationDto> setNewOrganization(OrganizationDto newOrganization)
        {
            var organization = await _organizationRepository.GetOrganizationById(newOrganization.OrganizationId);
            if (organization == null)
            {
                return null;
            }

            organization.Name = newOrganization.Name;
            organization.Email = newOrganization.Email;
            organization.PhoneNumber = newOrganization.PhoneNumber;
            return organization;
        }
        
        [HttpPost]
		[Authorize(Roles = "TeamLead")]
		public async Task<ActionResult> ChangePlan()
        {
			var cookieValue = Request.Cookies["SaaSPirateMarker"];
			var isMatch = string.Equals(cookieValue, "visited_page", StringComparison.Ordinal);

			var organizationCode = this.GetOrganizationCode();
            var organizationId = Guid.Parse(organizationCode);
            var currentTariffPlanHistory = await _tariffPlanHistoryRepository.RetrieveActive(organizationId);
            var tariffPlans = await _tariffPlanRepository.RetrievePlansList(
                currentTariffPlanHistory.TariffPlanCode, isMatch ? SD.TariffPlanSource : null);

            var GumroadPlanInfo = new Dictionary<string, SD.Gumroad.ProductType>()
            {
                { SD.StarterPlanCode, SD.Gumroad.ProductType.StarterPlan },
                { SD.ProPlanCode, SD.Gumroad.ProductType.ProPlan },
                { SD.BusinessPlanCode, SD.Gumroad.ProductType.BusinessPlan },
                { SD.EnterprisePlanCode, SD.Gumroad.ProductType.EnterprisePlan }
            };

            var plansWithLink = new List<PlanValues>();
            foreach (var plan in tariffPlans)
            {
                var tariffPlan = new PlanValues(plan);
                if (plan.Code != SD.BasicPlanCode)
                {
                    if (GumroadPlanInfo.TryGetValue(plan.Code, out var productType))
                    {
                        var gumroadLink = _gumroadLinkProvider.GetGumroadLink(productType, organizationId);
                        tariffPlan.SubscribeLink = gumroadLink;
                    }

                    if (currentTariffPlanHistory.TariffPlanCode == plan.Code)
                    {
                        tariffPlan.ExpirationDate = currentTariffPlanHistory.DateTo;
                    }
                }
                plansWithLink.Add(tariffPlan);
            }

            if (plansWithLink == null)
            {
                return Json(new ServerResponse(false) { Message = _localizer["NoInfoAboutTariffPlans"] });
            }

            ViewBag.OrganizationCode = organizationCode;
            ViewBag.IsSubscribe = currentTariffPlanHistory.TariffPlanCode != SD.BasicPlanCode 
                && currentTariffPlanHistory.DateTo > DateTime.UtcNow;

			return PartialView("_ChangePlan", plansWithLink);
        }

		[HttpPost]
        public async Task<ActionResult> AddUser(CreateUserModel model)
        {
            if (!ModelState.IsValid || model == null)
            {
                return Json(new { Message = _localizer["InvalidModelNullModel"] });
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

        [HttpPost]
        public async Task<JsonResult> DeleteUser(string userId)
        {
            if (!userId.Equals(null))
            {
                var taskResponse = await _taskRepository.DeleteAssigneeFromAllTasks(userId);
                if (taskResponse)
                {
                    var membershipResponse = await _membershipRepository.DeleteFromAllTeam(userId);
                    if (membershipResponse)
                    {
                        var isSuccess = await _applicationUserRepository.DeleteUser(userId);
                        if (isSuccess)
                        {
                            return Json(new ServerResponse(isSuccess) { Message = _localizer["DeleteUserSuccessful"] });
                        }    
                    }
                }
                return Json(new ServerResponse(false) { Message = _localizer["DeleteUserWasNotSuccessful"] });
            }
            else
            {
                return Json(new ServerResponse(false) { Message = _localizer["IdUserEqualsNull"] });
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateOrganizationRole(string userId, OrganizationRoles organizationRole)
        {
            if (!string.IsNullOrEmpty(userId) && organizationRole != null)
            {
                var isSuccess = await _applicationUserRepository.SetRoleInOrganizationForUser(userId, organizationRole);

                if (isSuccess)
                {
                    return Json(new ServerResponse(isSuccess)
                    {
                        Message = _localizer["TheRoleOrganizationSuccessfullyUpdated"]
                    });
                }
                else
                {
                    return Json(new ServerResponse(isSuccess)
                    {
                        Message = _localizer["ErrorUpdatingRoleOrganization"]
                    });
                }
            }
            else
            {
                return Json(new ServerResponse(false)
                {
                    Message = _localizer["IdOrganizationEmpty"]
                });
            }
        }
        [HttpPost]
        public async Task<ActionResult> SendToUs(OrganizationAppealDto appeal)
        {
            if (appeal != null)
            {
                var result = await _organizationAppealRepository.Insert(appeal);
                if (result)
                {
                    return Json(new ServerResponse(result) { Message = _localizer["ThankYouForYourRequestReview"] });
                }
                else
                {
                    return Json(new ServerResponse(result) { Message = _localizer["YourOperationWasNotSuccessful"] });
                }
            }

            return Json(new ServerResponse(false) { Message = _localizer["YourOperationWasNotSuccessful"] });
        }

		[HttpPost]
		public async Task<JsonResult> SwitchDeleteUser(string userId)
		{
			if (!userId.Equals(null))
			{
                var roleId = await getRoleTeamLeadId();

				var result = await _applicationUserRepository.SetRoleUser(userId, roleId);
				if (!result)
                {
					return Json(new ServerResponse(result) { Message = _localizer["SwitchRoleWasNotSuccessful"] });
				}

                await _applicationUserRepository.SetRoleInOrganizationForUser(userId, OrganizationRoles.TeamLead);

				var thisUser = this.GetUserId();
				var isSuccess = await _applicationUserRepository.DeleteUser(thisUser);
				if (isSuccess)
				{
					await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

                    return Json(new ServerResponse(true)
                    {
                        RedirectUrl = Url.Action("Index", "Home")
                    });
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

        private async Task<string?> getRoleTeamLeadId()
        {
			var roles = await _applicationUserRepository.GetRoles();
			var roleId = roles
				.Where(r => r.Name == SD.TeamLead)
				.Select(r => r.Id)
				.FirstOrDefault();
            return roleId;
		}

		[HttpGet]
		public async Task<JsonResult> DeleteMyOrganization()
		{
            var organizationCode = this.GetOrganizationCode();

			var result = await _organizationRepository.DeleteOrganization(organizationCode);
			if (result)
			{
				await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

				return Json(new ServerResponse(true)
				{
					RedirectUrl = Url.Action("Index", "Home")
				});
			}
			else
			{
				return Json(new ServerResponse(result) { Message = _localizer["DeleteOrganizationWasNotSuccessful"] });
			}
		}

		private ActionResult redirectToErrorPage(string errorTitle, string errorMsg)
        {
            TempData["ErrorTitle"] = errorTitle;
            TempData["ErrorMessage"] = errorMsg;
            return RedirectToAction("Error", "Organization");
        }

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
