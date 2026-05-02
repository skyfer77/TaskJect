using Domain.Database;
using Domain.Enums;
using Data;
using Domain.IServices;
using TaskJect.Web.Models;
using TaskJect.Web.Resources;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TaskJect.Web.Services;
using AutoMapper;
using TaskJect.Web.Enums;

namespace TaskJect.Web.Controllers
{
    [Authorize(Roles = "Moderator, God, Admin")]
    public class ModeratorController : Controller
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IOrganizationAppealRepository _organizationAppealRepository;
        private readonly ITariffPlanHistoryRepository _tariffPlanHistoryRepository;
        private readonly ITariffPlanRepository _tariffPlanRepository;
        private readonly IStringLocalizer<ErrorResources> _localizer;
        private readonly IUserCreator _userCreator;
        private readonly IOrganizationLimitationsEnforcer _organizationLimitationsEnforcer;
        private IMapper _mapper;
	     public ModeratorController(IOrganizationRepository organizationRepository, 
            IApplicationUserRepository applicationUserRepository, IOrganizationAppealRepository organizationAppealRepository, 
            ITariffPlanHistoryRepository tariffPlanHistoryRepository, ITariffPlanRepository tariffPlanRepository,
            IStringLocalizer<ErrorResources> localizer, IUserCreator userCreator, IOrganizationLimitationsEnforcer organizationLimitationsEnforcer,IMapper mapper)
        {
            _organizationRepository = organizationRepository;
            _applicationUserRepository = applicationUserRepository;
            _organizationAppealRepository = organizationAppealRepository;
            _tariffPlanHistoryRepository = tariffPlanHistoryRepository;
            _tariffPlanRepository = tariffPlanRepository;
            _localizer = localizer;
            _userCreator = userCreator;
            _organizationLimitationsEnforcer = organizationLimitationsEnforcer;
            _mapper = mapper;
		}

        #region Organization
        [HttpGet]
        public async Task<ActionResult> InviteOrganization()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateOrganization(CreateOrganizationModel model)
        {
            if (!ModelState.IsValid || model == null)
            {
                return Json(new ServerResponse(false){ Message = _localizer["InvalidModelNullModel"] });
            }

            var organizationDto = new OrganizationDto()
            {
                Name = model.Name,
            };       
            var result = await _organizationRepository.Insert(organizationDto);
            if (result)
            {
                var organization = await _organizationRepository.GetOrganizationByName(model.Name);
                if (organization != null)
                {
                    var organizationId = organization.OrganizationId.ToString();
                    if (!string.IsNullOrEmpty(organizationId))
                    {
                        var teamLead = new CreateUserByEmailModel()
                        {
                            OrganizationCode = organizationId,
                            Email = model.Email,
                            FirstName = model.FirstName,
                            Surname = model.Surname,
                            RoleUser = "TeamLead"
                        };

                        var isSuccess = await _userCreator.CreateUser(teamLead);

                        if (isSuccess)
                        {
                            var newTariffPlanHistory = new TariffPlanHistoryDto();
                            newTariffPlanHistory.OrganizationCode = organization.OrganizationId;
                            newTariffPlanHistory.TariffPlanCode = SD.BasicPlanCode;
                            newTariffPlanHistory.DateFrom = DateTime.UtcNow.Date;
                            newTariffPlanHistory.DateTo = new DateTime(9999, 12, 31, 23, 59, 59);
                            var tariffHistoryResponse = await _tariffPlanHistoryRepository.Insert(newTariffPlanHistory);
                            if(tariffHistoryResponse)
                            { 
                                return Json(new ServerResponse(true) { Message = _localizer["CreateNewOrganizationUserSuccessful"] });
                            }
                            else
                            {
                                return Json(new ServerResponse(true) { Message = _localizer["CreatingNewOrganizationWasSuccessful"] });
                            }
                        }
                        else
                        {
                            return Json(new ServerResponse(false) { Message = _localizer["CreatingNewOrganizationUserWasSuccessful"] });
                        }
                    }
                    else
                    {
                        return Json(new ServerResponse(false) { Message = _localizer["IdOrganizationEmpty"] });
                    }
                }
                else
                {
                    return Json(new ServerResponse(false) { Message = _localizer["OrganizationNotFound"] });
                }
            }
            else
            {
                return Json(new ServerResponse(false) { Message = _localizer["CreateOrganizationWasSuccessfulAlreadyExists"] });
            }
        }

        [HttpGet]
        public async Task<ActionResult> Organizations()
        {
            var organizations = await _organizationRepository.Retrieve();
            if (organizations == null)
            {
                return redirectToErrorPage(_localizer["organizationsLoadErrorTitle"], _localizer["organizationsLoadErrorMessage"]);
            }

            var usersInfo = await _applicationUserRepository.GetOrganizationUserInfo();
        
            if (usersInfo == null)
            {
                return redirectToErrorPage(_localizer["organizationsUserInfoLoadErrorTitle"], _localizer["organizationsLoadUserInfoErrorMessage"]);
            }

            var listOrganization = new List<OrganizationViewModel>();

            foreach (var organization in organizations)
            {
                var userInfo = usersInfo.FirstOrDefault(ui => ui.OrganizationId.ToLower().Equals(organization.OrganizationId.ToString()));
                var tariffPlan = await _tariffPlanHistoryRepository.RetrieveActive(organization.OrganizationId);
                if(tariffPlan == null)
                {
                    continue;
                }

                listOrganization.Add(new OrganizationViewModel
                {
                    Id = organization.OrganizationId,
                    Name = organization.Name,
                    Picture = organization.Picture,
                    RegistrationDate = organization.RegistrationDate,
                    LockoutEnabled = organization.LockoutEnabled,
                    LockoutEnd = organization.LockoutEnd,
                    CurrentPlanCode = tariffPlan.TariffPlanCode,
                    CurrentPlanDateTo = tariffPlan.DateTo,
                    CountOfParticipants = userInfo?.CountUserOrganization ?? 0,
                    TeamLead = userInfo?.TeamLead,
                });
            }

            return View(listOrganization);
        }
        public async Task<ActionResult> HistoryPlans(Guid organizationId)
        {

            if (organizationId != null && organizationId != Guid.Empty)
            {
                var listHistoryTariff = new List<TariffUpdateModel>();
                var history =  await _tariffPlanHistoryRepository.RetrieveByOrganization(organizationId);
                if (history == null)
                {
                    return Json(new ServerResponse(false) { Message = _localizer["ThereOrganizationThisId"] });
                }
                foreach (var tariff in history)
                {
                    listHistoryTariff.Add(new TariffUpdateModel
                    {
                       OrganizationId = tariff.OrganizationCode,
                       TariffDateFrom = tariff.DateFrom,
                       TariffDateTo = tariff.DateTo,
                       TariffName = tariff.TariffPlanCode,
                    });
                }
                listHistoryTariff = listHistoryTariff.OrderByDescending(t => t.TariffDateTo).ToList();
                return View(listHistoryTariff);
            }
            else
            {
                return Json(new ServerResponse(false) { Message = _localizer["IdOrganizationEqualsNull"] });
            }
            
        }
        [HttpGet]
        public async Task<ActionResult> GetTariffModal(string organizationCode, string dateTo, string tariffName)
        {
            ViewBag.CurrentTariff = tariffName;
            ViewBag.OrganizationCode = organizationCode;
            ViewBag.DateTo = dateTo;
            ViewBag.TariffPlans = await _tariffPlanRepository.Retrieve();

            return PartialView("_UpdateTariffPlan");
        }
        [HttpPost]
        public async Task<JsonResult> UpdateTariffDate(TariffUpdateModel model)
        {
            if (!ModelState.IsValid || model == null)
            {
                return Json(new ServerResponse(false) { Message = _localizer["InvalidModelNullModel"] });
            }
            var currentTariff = await _tariffPlanHistoryRepository.RetrieveActive(model.OrganizationId);
            if(currentTariff.TariffPlanCode == model.TariffName && currentTariff.DateTo.Date == model.TariffDateTo.Date)
            {
                return Json(new ServerResponse(false) { Message = _localizer["CurrentTariffSame"] });
            }
            else
            {
                var tariff = new TariffPlanHistoryDto();
                tariff.OrganizationCode = model.OrganizationId;
                tariff.TariffPlanCode = model.TariffName;
                tariff.DateFrom = DateTime.UtcNow;
                tariff.DateTo = (model.TariffDateTo <= tariff.DateFrom) ? tariff.DateFrom.AddMonths(1) : model.TariffDateTo;
                var result = await _organizationLimitationsEnforcer.ApplyTariffPlan(tariff , false);
                if (result)
                {
                    await _organizationLimitationsEnforcer.UnlockUsers(model.OrganizationId.ToString(), model.TariffName);
                    return Json(new ServerResponse(true) { Message = _localizer["CurrentTariffChanged"] });
                }
                else
                {
                    return Json(new ServerResponse(false) { Message = _localizer["TariffPlanNotUpdated"] });
                }
            }
        }
        [HttpPost]
        public async Task<ActionResult> Lockout(string Id)
        {
            if (!Id.Equals(null))
            {
                var organizationId = Guid.Parse(Id);
                bool isLockout = true;
                var result = await _organizationRepository.LockoutUnlockout(isLockout, organizationId);
                if (result)
                {
                    DateTime? lockoutEnd = null;
                    var isSuccess = await _applicationUserRepository.LockoutUser(lockoutEnd, Id);
                    if (isSuccess)
                    {
                        return Json(new ServerResponse(isSuccess) { Message = _localizer["YourOperationSuccessful"] });
                    }
                    else
                    {
                        return Json(new ServerResponse(isSuccess) {  Message = _localizer["OrganizationLockoutUsersNotLockout"] });
                    }
                }
                else
                {
                    return Json(new ServerResponse(result) { Message = _localizer["YourOperationWasNotSuccessful"] });
                }
            }
            else
            {
                return Json(new ServerResponse(false) { Message = _localizer["IdOrganizationEqualsNull"] });
            }
        }

        [HttpPost]
        public async Task<ActionResult> Unlockout(string Id)
        {
            if (Id != null)
            {
                bool isLockout = false;
                var organizationId = Guid.Parse(Id);
                var result = await _organizationRepository.LockoutUnlockout(isLockout, organizationId);
                if (result)
                {
                    var isSuccess = await _applicationUserRepository.UnlockoutAllUser(Id);
                    if (isSuccess)
                    {
                        return Json(new ServerResponse(isSuccess) { Message = _localizer["YourOperationSuccessful"] });
                    }
                    else
                    {
                        return Json(new ServerResponse(isSuccess) { Message = _localizer["OrganizationUnlockedUsersNotUnlocked"] });
                    }
                }
                else
                {
                    return Json(new ServerResponse(result) { Message = _localizer["YourOperationWasNotSuccessful"] });
                }
            }
            else
            {
                return Json(new ServerResponse(false) { Message = _localizer["IdOrganizationEqualsNull"] });
            }
        }

        [HttpPost]
        public async Task<JsonResult> DeleteOrganization(string organizationId)
        {
            if (!string.IsNullOrEmpty(organizationId))
            {
                var result = await _organizationRepository.DeleteOrganization(organizationId);
				if (result)
				{
					return Json(new ServerResponse(result) { Message = _localizer["DeleteOrganizationSuccessful"] });
				}
				else
				{
					return Json(new ServerResponse(result) { Message = _localizer["DeleteOrganizationWasNotSuccessful"] });
				}
            }
            else
            {
                return Json(new ServerResponse(false) { Message = _localizer["IdOrganizationEqualsNull"] });
            }
        }

        [HttpGet]
        public async Task<ActionResult> UsersOrganization(string organizationId)
        {
            if (string.IsNullOrEmpty(organizationId))
            {
                return View();
            }

            var organizationIdGuid = Guid.Parse(organizationId);

            var organization = await _organizationRepository.GetOrganizationById(organizationIdGuid);
            if(organization == null)
            {
                return redirectToErrorPage(_localizer["organizationLoadErrorTitle"], _localizer["organizationLoadErrorMessage"]);
            }

            var users = await _applicationUserRepository.GetAllUsersTheOrganization(organizationId);
            
            if (users == null)
            {
                return redirectToErrorPage(_localizer["NoFoundDetails"], _localizer["NoFoundCurrentUser"]);
            }

            var roles = await _applicationUserRepository.GetRoles();
            if (roles == null)
            {
                return redirectToErrorPage(_localizer["rolesLoadErrorTitle"], _localizer["rolesLoadErrorMessage"]);
            }

            var usersOrganization = new UsersInTheOrganizationModel()
            {
                Organization = organization,
                Users = users,
                Roles = roles,
            };

            return View(usersOrganization);
        }

        #endregion

        #region User
        [HttpPost]
        public async Task<JsonResult> SetNewRoleForUser(string userId, string roleId)
        {
            if (!userId.Equals(null) && !roleId.Equals(null))
            {
                var accessToken = await HttpContext.GetTokenAsync("access_token");
                var isSuccess = await _applicationUserRepository.SetRoleUser(userId, roleId);
                if (isSuccess)
                {
                    return Json(new ServerResponse(isSuccess) { Message = _localizer["RoleUpdateSuccessful"] });
                }
                else
                {
                    return Json(new ServerResponse(isSuccess) { Message = _localizer["RoleUpdateWasNotSuccessful"] });
                }
            }
            else
            {
                return Json(new ServerResponse(false) { Message = _localizer["IdUserRoleEqualsNull"] });
            }
        }

        [HttpPost]
        public async Task<JsonResult> LockoutUser(string userId)
        {
            if (!userId.Equals(null))
            {
                DateTime? lockoutEnd = DateTime.MaxValue;
                var isSuccess = await _applicationUserRepository.LockoutUser(lockoutEnd, userId);
                if (isSuccess)
                {
                    return Json(new ServerResponse(isSuccess) { Message = _localizer["LockoutUserSuccessful"] });
                }
                else
                {
                    return Json(new ServerResponse(isSuccess) { Message = _localizer["LockoutUserWasNotSuccessful"] });
                }
            }
            else
            {
                return Json(new ServerResponse(false) { Message = _localizer["IdUserEqualsNull"] });
            }
        }

        [HttpPost]
        public async Task<JsonResult> UnlockUser(string userId)
        {
            if (!userId.Equals(null))
            {
                var isSuccess = await _applicationUserRepository.UnlockoutUser(userId);
                if (isSuccess)
                {
                    return Json(new ServerResponse(isSuccess) { Message = _localizer["UnlockUserSuccessful"] });
                }
                else
                {
                    return Json(new ServerResponse(isSuccess) { Message = _localizer["UnlockUserWasNotSuccessful"] });
                }
            }
            else
            {
                return Json(new ServerResponse(false) { Message = _localizer["IdUserEqualsNull"] });
            }
        }

        [HttpPost]
        public async Task<JsonResult> DeleteUser(string userId)
        {
            if (!userId.Equals(null))
            {
                var isSuccess = await _applicationUserRepository.DeleteUser(userId);
                if (isSuccess)
                {
                    return Json(new ServerResponse(isSuccess) { Message = _localizer["DeleteUserSuccessful"] });
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
        #endregion

        #region OrganizationAppeal
        [HttpGet]
        public async Task<ActionResult> OrganizationAppeals()
        {
			var organizations = await _organizationRepository.Retrieve();
            if (organizations == null)
            {
                return redirectToErrorPage(_localizer["organizationsLoadErrorTitle"], _localizer["organizationsLoadErrorMessage"]);
            }

            var organizationAppeals = await _organizationAppealRepository.Retrieve();
            if (organizationAppeals == null)
            {
                return redirectToErrorPage(_localizer["organizationAppealsLoadErrorTitle"], _localizer["organizationAppealsLoadErrorMessage"]);
            }

            var organizationDict = organizations.ToDictionary(org => org.OrganizationId, org => org);

            var result = new List<OrganizationAppealViewModel>();

            foreach (var appeal in organizationAppeals)
            {
                if (organizationDict.TryGetValue(appeal.OrganizationCode, out var organization))
                {
                    result.Add(new OrganizationAppealViewModel
                    {
                        Id = appeal.Id,
                        OrganizationId = organization.OrganizationId,
                        OrganizationName = organization.Name,
                        Picture = organization.Picture,
                        Title = appeal.Title,
                        Date = appeal.Date,
                        Status = _mapper.Map<AppealStatusView>(appeal.Status),
                    });
                }
            }

            return View(result);
        }

        [HttpPost]
        public async Task<ActionResult> ModalWindowEditAppeal(string appealId, string organizationId)
        {
            var organization = await _organizationRepository.GetOrganizationById(Guid.Parse(organizationId));
            if (organization == null)
            {
                return redirectToErrorPage(_localizer["organizationLoadErrorTitle"], _localizer["organizationLoadErrorMessage"]);
            }

            var organizationAppeal = await _organizationAppealRepository.Retrieve(Guid.Parse(appealId));
            if (organizationAppeal == null)
            {
                return redirectToErrorPage(_localizer["organizationAppealsLoadErrorTitle"], _localizer["organizationAppealsLoadErrorMessage"]);
            }
            
            var result = new OrganizationAppealViewModel
            {
                Id = organizationAppeal.Id,
                OrganizationId = organization.OrganizationId,
                OrganizationName = organization.Name,
                Email = organization.Email,
                PhoneNumber = organization.PhoneNumber,
                Picture = organization.Picture,
                Title = organizationAppeal.Title,
                Description = organizationAppeal.Description,
                Date = organizationAppeal.Date,
                Status = _mapper.Map<AppealStatusView>(organizationAppeal.Status),
                DescriptionRejecting = organizationAppeal.DescriptionRejecting
            };

            return PartialView("_EditAppeal", result);
        }

        [HttpPost]
        public async Task<ActionResult> EditAppeal(OrganizationAppealDto appeal)
        {
            if (appeal != null)
            {
                var newAppeal = await setNewStatusAppeal(appeal);
                if (newAppeal != null)
                {
                    var result = await _organizationAppealRepository.Update(newAppeal);
                    if (result)
                    {
                        return Json(new ServerResponse(result) { Message = _localizer["OrganizationAppealEditSuccessful"] });
                    }
                    else
                    {
                        return Json(new ServerResponse(result) { Message = _localizer["OrganizationAppealEditWasNotSuccessful"] });
                    }
                }
            }

            return Json(new ServerResponse(false) { Message = _localizer["OrganizationAppealEqualsNull"] });
        }

        private async Task<OrganizationAppealDto?> setNewStatusAppeal(OrganizationAppealDto appeal)
        {
            var appealDto = await _organizationAppealRepository.Retrieve(appeal.Id);
            if (appealDto == null)
            {
                return null;
            }

            appealDto.Status = appeal.Status;
            if (appealDto.Status == AppealStatus.Rejected)
            {
                appealDto.DescriptionRejecting = appeal.DescriptionRejecting;
            }
            else
            {
                appealDto.DescriptionRejecting = string.Empty;
            }

            return appealDto;
        }

        [HttpPost]
        public async Task<ActionResult> DeleteAppeal(string appealId)
        {
            if (!string.IsNullOrEmpty(appealId))
            {
                var result = await _organizationAppealRepository.Delete(Guid.Parse(appealId));
                if (result)
                {
                    return Json(new ServerResponse(result) { Message = _localizer["DeleteOrganizationAppealSuccessful"] });
                }
                else
                {
                    return Json(new ServerResponse(result) { Message = _localizer["DeleteOrganizationAppealWasNotSuccessful"] });
                }
            }
            else
            {
                return Json(new ServerResponse(false) { Message = _localizer["IdOrganizationEqualsNull"] });
            }
        }
        #endregion

        private ActionResult redirectToErrorPage(string errorTitle, string errorMsg)
        {
            TempData["ErrorTitle"] = errorTitle;
            TempData["ErrorMessage"] = errorMsg;
            return RedirectToAction("Error", "Moderator");
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
