using Domain.Database;
using TaskJect.Web.Models;
using TaskJect.Web.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using AutoMapper;
using TaskJect.Web.Enums;

namespace TaskJect.Web.Controllers
{
    public class ImprovementsController : Controller
    {
        private readonly IOrganizationAppealRepository _organizationAppealRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IStringLocalizer<ErrorResources> _localizer;
        private readonly IMapper _mapper;

        public ImprovementsController(IOrganizationAppealRepository organizationAppealRepository, 
            IOrganizationRepository organizationRepository, IStringLocalizer<ErrorResources> localizer, IMapper mapper)
        {
            _organizationAppealRepository = organizationAppealRepository;
            _organizationRepository = organizationRepository;
            _localizer = localizer;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult> OverviewImprovements()
        {
            var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;
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
                        Status = _mapper.Map<AppealStatusView>(appeal.Status)
                    });
                }
            }
            Guid organizationGuid;
            if (Guid.TryParse(organizationCode, out organizationGuid))
            {
                ViewBag.OrganizationCode = organizationGuid;
            }
            else
            {
                ViewBag.OrganizationCode = null; 
            }
            return View(result);
        }

        [HttpGet]
        public async Task<ActionResult> GetImprovementsModal(Guid idAppeal)
        {
            var thisAppeal = await _organizationAppealRepository.Retrieve(idAppeal);
            var result = new OrganizationAppealViewModel
            {
               Title = thisAppeal.Title,
               Description = thisAppeal.Description
            };

            return PartialView("_DetailImprovements", result);
        }
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
