using Microsoft.AspNetCore.Mvc;

namespace TaskJect.Web.Controllers
{
    public class FinanceController : Controller
    {
        /*IFinanceService _financeService;
        IProjectRepository _projectRepository;
        IApplicationUserLiteService _applicationUserLiteService;
        public FinanceController(IFinanceService financeService, IProjectRepository projectRepository, IApplicationUserLiteService applicationUserLiteService)
        {
            _financeService = financeService;
            _projectRepository = projectRepository;
            _applicationUserLiteService = applicationUserLiteService;
        }
        [Authorize(Roles = "Admin, God")]
        public async Task<IActionResult> Index()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            ViewBag.token = accessToken;
            var financeViewModel = new FinanceViewModel();
            var projectsName = new List<ProjectModel>();
            var usersName = new List<UserModel>();

            var costsResponse = await _financeService.GetCostsAsync<ResponseDto>(accessToken, null, null);
            if(costsResponse != null && costsResponse.IsSuccess)
            {
                var costs = JsonConvert.DeserializeObject<List<CostDto>>(costsResponse.Result.ToString());
                if(costs != null)
                {
                    financeViewModel.Costs = costs;
                }
            }
            else if (costsResponse == null)
            {
                return redirectToErrorPage("Controller Finance not found", "Metod Index costsResponse is null!");
            }

            var incomesResponse = await _financeService.GetIncomesAsync<ResponseDto>(accessToken, null, null);
            if (incomesResponse != null && incomesResponse.IsSuccess)
            {
                var incomes = JsonConvert.DeserializeObject<List<IncomeDto>>(incomesResponse.Result.ToString());
                if (incomes != null)
                {
                    financeViewModel.Incomes = incomes;
                }
            }
            else if (incomesResponse == null)
            {
                return redirectToErrorPage("Controller Finance not found", "Metod Index incomesResponse is null!");
            }

            var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;
            var projects = await _projectRepository.RetrieveByOrganization(organizationCode);
            if (projects == null)
            {
                return redirectToErrorPage("Controller Finance not found", "Metod Index projects is null!");
            }

            foreach (var project in projects)
            {
                projectsName.Add(new ProjectModel() { Id = project.ID.ToString(), Name = project.Title });
            }
            financeViewModel.Projects = projectsName;

            var usersResponse = await _applicationUserLiteService.GetAllUsersLiteTheOrganizationAsync<ResponseDto>(accessToken, organizationCode);
            if (usersResponse != null && usersResponse.IsSuccess)
            {
                var users = JsonConvert.DeserializeObject<List<ApplicationUserLiteDto>>(usersResponse.Result.ToString());
                if (users != null)
                {
                    foreach (var user in users)
                    {
                        usersName.Add(new UserModel() { UserId = user.Id.ToString(), UserName = user.Name, UserSurname = user.Surname });
                    }
                    financeViewModel.Users = usersName;
                }
            }
            else if (usersResponse == null)
            {
                return redirectToErrorPage("Controller Finance not found", "Metod Index usersResponse is null!");
            }

            return View(financeViewModel);
        }

        #region Income
        [Authorize(Roles = "Admin, God")]
        public async Task<IActionResult> OverviewIncome(Guid id)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var incomesDetail = new IncomeDetailsModel();
            var projectsName = new List<ProjectModel>();

            var incomesDetailResponse = await _financeService.GetIncomeDetailsAsync<ResponseDto>(id, accessToken);
            if (incomesDetailResponse != null && incomesDetailResponse.IsSuccess)
            {
                var incomes = JsonConvert.DeserializeObject<List<IncomeDetailDto>>(incomesDetailResponse.Result.ToString());
                if (incomes != null)
                {
                    incomesDetail.IncomeDetail = incomes;
                }
            }
            else if (incomesDetailResponse == null)
            {
                return redirectToErrorPage("Controller Finance not found", "Metod OverviewIncome incomesDetailResponse is null!");
            }

            var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;
            var projects = await _projectRepository.RetrieveByOrganization(organizationCode);
            if (projects == null)
            {
                return redirectToErrorPage("Controller Finance not found", "Metod OverviewIncome projects is null!");
            }

            foreach (var project in projects)
            {
                projectsName.Add(new ProjectModel() { Id = project.ID.ToString(), Name = project.Title });
            }
            incomesDetail.Projects = projectsName;

            return View(incomesDetail);
        }

        [Authorize(Roles = "Admin, God")]
        public async Task<JsonResult> CreateIncome(IncomeDto incomeDto, string sum)
        {
            if (incomeDto != null)
            {
                incomeDto.Amount = double.Parse(sum, CultureInfo.InvariantCulture);
                incomeDto.Id = Guid.NewGuid();
                var accessToken = await HttpContext.GetTokenAsync("access_token");

                var result = await _financeService.AddIncomeAsync<ResponseDto>(incomeDto, accessToken);

                if (result == null)
                {
                    TempData["ErrorTitle"] = "Controller Finance not found";
                    TempData["ErrorMessage"] = result.ErrorMessages[0];
                    //success 0 redirect to Error controller 
                    return Json(new { success = 0 });
                }
                else if (!result.IsSuccess)
                {
                    //  Send "false"
                    return Json(new ServerResponse(result.IsSuccess){ Message = result.ErrorMessages[0] });
                }
                else
                {
                    //  Send "Success"
                    return Json(new ServerResponse(result.IsSuccess){ Message = "Your operation is successful!"});
                }
            }
            else
            {
                //  Send "false"
                return Json(new ServerResponse(false){ Message = "IncomeDto model equals null!" });
            }
        }

        [Authorize(Roles = "Admin, God")]
        [HttpPost]
        public async Task<IActionResult> EditIncome(Guid id)
        {
            var projectsName = new List<ProjectModel>();
            var incomes = new List<IncomeDto>();
            var income = new IncomeDto();
            var accessToken = await HttpContext.GetTokenAsync("access_token");


            var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;
            var projects = await _projectRepository.RetrieveByOrganization(organizationCode);
            if (projects == null)
            {
                return redirectToErrorPage("Controller Finance not found", "Metod EditIncome projects is null!");
            }

            foreach (var project in projects)
            {
                projectsName.Add(new ProjectModel() { Id = project.ID.ToString(), Name = project.Title });
            }

            var incomesResponse = await _financeService.GetIncomesAsync<ResponseDto>(accessToken, DateTime.MinValue, DateTime.MaxValue);
            if (incomesResponse != null && incomesResponse.IsSuccess)
            {
                incomes = JsonConvert.DeserializeObject<List<IncomeDto>>(incomesResponse.Result.ToString());
                if (incomes != null)
                {
                    income = incomes.FirstOrDefault(x => x.Id == id);
                }
            }
            else if (incomesResponse == null)
            {
                return redirectToErrorPage("Controller Finance not found", "Metod EditIncome incomesResponse is null!");
            }

            var incomeEdit = new IncomeEditModel()
            {
                Income = income,
                Projects = projectsName
            };

            return PartialView("_EditIncomeRow", incomeEdit);
        }

        [Authorize(Roles = "Admin, God")]
        [HttpPost]
        public async Task<JsonResult> UpdateIncome(IncomeDto incomeDto, string sum)
        {
            if (incomeDto != null)
            {
                incomeDto.Amount = double.Parse(sum, CultureInfo.InvariantCulture);

                var accessToken = await HttpContext.GetTokenAsync("access_token");

                var result = await _financeService.UpdateIncomeAsync<ResponseDto>(incomeDto, accessToken);

                if (result == null)
                {
                    TempData["ErrorTitle"] = "Controller Finance not found";
                    TempData["ErrorMessage"] = result.ErrorMessages[0];
                    //success 0 redirect to Error controller 
                    return Json(new { success = 0 });
                }
                else if (!result.IsSuccess)
                {
                    //  Send "false"
                    return Json(new ServerResponse(result.IsSuccess) { Message = result.ErrorMessages[0] });
                }
                else
                {
                    //  Send "Success"
                    return Json(new ServerResponse(result.IsSuccess) { Message = "Your operation is successful!" });
                }
            }
            else
            {
                //  Send "false"
                return Json(new ServerResponse(false) { Message = "IncomeDto model equals null!" });
            }

        }

        [Authorize(Roles = "Admin, God")]
        [HttpPost]
        public async Task<JsonResult> DeleteIncome(Guid id)
        {
            if (!id.Equals(null))
            {
                var accessToken = await HttpContext.GetTokenAsync("access_token");

                var result = await _financeService.DeleteIncomeAsync<ResponseDto>(id, accessToken);

                if (result == null)
                {
                    TempData["ErrorTitle"] = "Controller Finance not found";
                    TempData["ErrorMessage"] = result.ErrorMessages[0];
                    //success 0 redirect to Error controller 
                    return Json(new { success = 0 });
                }
                else if (!result.IsSuccess)
                {
                    //  Send "false"
                    return Json(new ServerResponse(result.IsSuccess) { Message = result.ErrorMessages[0] });
                }
                else
                {
                    //  Send "Success"
                    return Json(new ServerResponse(result.IsSuccess) {  Message = "Your operation is successful!" });
                }
            }
            else
            {
                //  Send "false"
                return Json(new ServerResponse(false) { Message = "Id income equals null!" });
            }
        }

        #endregion

        #region Cost

        [Authorize(Roles = "Admin, God")]
        public async Task<IActionResult> OverviewCost(Guid id)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var costsDetail = new CostDetailsModel();
            var projectsName = new List<ProjectModel>();
            var usersName = new List<UserModel>();

            var costsDetailResponse = await _financeService.GetCostDetailsAsync<ResponseDto>(id, accessToken);
            if (costsDetailResponse != null && costsDetailResponse.IsSuccess)
            {
                var costs = JsonConvert.DeserializeObject<List<CostDetailDto>>(costsDetailResponse.Result.ToString());
                if (costs != null)
                {
                    costsDetail.CostDetail = costs;
                }
            }
            else if (costsDetailResponse == null)
            {
                return redirectToErrorPage("Controller Finance not found", "Metod OverviewCost costsDetailResponse is null!");
            }

            var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;
            var projects = await _projectRepository.RetrieveByOrganization(organizationCode);
            if (projects == null)
            {
                return redirectToErrorPage("Controller Finance not found", "Metod OverviewCost projects is null!");
            }

            foreach (var project in projects)
            {
                projectsName.Add(new ProjectModel() { Id = project.ID.ToString(), Name = project.Title });
            }
            costsDetail.Projects = projectsName;

            var usersResponse = await _applicationUserLiteService.GetAllUsersLiteTheOrganizationAsync<ResponseDto>(accessToken, organizationCode);
            if (usersResponse != null && usersResponse.IsSuccess)
            {
                var users = JsonConvert.DeserializeObject<List<ApplicationUserLiteDto>>(usersResponse.Result.ToString());
                if (users != null)
                {
                    foreach (var user in users)
                    {
                        usersName.Add(new UserModel() { UserId = user.Id.ToString(), UserName = user.Name, UserSurname = user.Surname });
                    }
                    costsDetail.Users = usersName;
                }
            }
            else if (usersResponse == null)
            {
                return redirectToErrorPage("Controller Finance not found", "Metod OverviewCost usersResponse is null!");
            }

            return View(costsDetail);
        }

        [Authorize(Roles = "Admin, God")]
        [HttpPost]
        public async Task<JsonResult> CreateCost(CostDto costDto, string sum)
        {
            if (costDto != null)
            {
                costDto.Amount = double.Parse(sum, CultureInfo.InvariantCulture);
                costDto.Id = Guid.NewGuid();
                var accessToken = await HttpContext.GetTokenAsync("access_token");

                var result = await _financeService.AddCostAsync<ResponseDto>(costDto, accessToken);

                if (result == null)
                {
                    TempData["ErrorTitle"] = "Controller Finance not found";
                    TempData["ErrorMessage"] = result.ErrorMessages[0];
                    //success 0 redirect to Error controller 
                    return Json(new { success = 0 });
                }
                else if (!result.IsSuccess)
                {
                    //  Send "false"
                    return Json(new ServerResponse(result.IsSuccess) { Message = result.ErrorMessages[0] });
                }
                else
                {
                    //  Send "Success"
                    return Json(new ServerResponse(result.IsSuccess) { Message = "Your operation is successful!" });
                }
            }
            else
            {
                //  Send "false"
                return Json(new ServerResponse(false) { Message = "CostDto model equals null!" });
            }
        }

        [Authorize(Roles = "Admin, God")]
        [HttpPost]
        public async Task<IActionResult> EditCost(Guid id)
        {
            var projectsName = new List<ProjectModel>();
            var usersName = new List<UserModel>();
            var cost = new CostDto();
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var organizationCode = User.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;
            var projects = await _projectRepository.RetrieveByOrganization(organizationCode);
            if (projects == null)
            {
                return redirectToErrorPage("Controller Finance not found", "Metod EditCost projects is null!");
            }

            foreach (var project in projects)
            {
                projectsName.Add(new ProjectModel() { Id = project.ID.ToString(), Name = project.Title });
            }

            var usersResponse = await _applicationUserLiteService.GetAllUsersLiteTheOrganizationAsync<ResponseDto>(accessToken, organizationCode);
            if (usersResponse != null && usersResponse.IsSuccess)
            {
                var users = JsonConvert.DeserializeObject<List<ApplicationUserLiteDto>>(usersResponse.Result.ToString());
                if (users != null)
                {
                    foreach (var user in users)
                    {
                        usersName.Add(new UserModel() { UserId = user.Id.ToString(), UserName = user.Name, UserSurname = user.Surname });
                    }
                }
            }
            else if (usersResponse == null)
            {
                return redirectToErrorPage("Controller Finance not found", "Metod EditCost usersResponse is null!");
            }

            var costsResponse = await _financeService.GetCostsAsync<ResponseDto>(accessToken, DateTime.MinValue, DateTime.MaxValue);
            if (costsResponse != null && costsResponse.IsSuccess)
            {
                var costs = JsonConvert.DeserializeObject<List<CostDto>>(costsResponse.Result.ToString());
                if (costs != null)
                {
                    cost = costs.FirstOrDefault(x => x.Id == id);
                }
            }
            else if (costsResponse == null)
            {
                return redirectToErrorPage("Controller Finance not found", "Metod EditCost costsResponse is null!");
            }

            var costEdit = new CostEditModel()
            {
                Cost = cost,
                Projects = projectsName,
                Users = usersName
            };

            return PartialView("_EditCostRow", costEdit);
        }

        [Authorize(Roles = "Admin, God")]
        [HttpPost]
        public async Task<JsonResult> UpdateCost(CostDto costDto, string sum)
        {
            if (costDto != null)
            {
                costDto.Amount = double.Parse(sum, CultureInfo.InvariantCulture);

                var accessToken = await HttpContext.GetTokenAsync("access_token");

                var result = await _financeService.UpdateCostAsync<ResponseDto>(costDto, accessToken);

                if (result == null)
                {
                    TempData["ErrorTitle"] = "Controller Finance not found";
                    TempData["ErrorMessage"] = result.ErrorMessages[0];
                    //success 0 redirect to Error controller 
                    return Json(new { success = 0 });
                }
                else if (!result.IsSuccess)
                {
                    //  Send "false"
                    return Json(new ServerResponse(result.IsSuccess) { Message = result.ErrorMessages[0] });
                }
                else
                {
                    //  Send "Success"
                    return Json(new ServerResponse(result.IsSuccess) { Message = "Your operation is successful!" });
                }
            }
            else
            {
                //  Send "false"
                return Json(new ServerResponse(false) { Message = "CostDto model equals null!" });
            }

        }

        [Authorize(Roles = "Admin, God")]
        [HttpPost]
        public async Task<JsonResult> DeleteCost(Guid id)
        {
            if (!id.Equals(null))
            {
                var accessToken = await HttpContext.GetTokenAsync("access_token");

                var result = await _financeService.DeleteCostAsync<ResponseDto>(id, accessToken);

                if (result == null)
                {
                    TempData["ErrorTitle"] = "Controller Finance not found";
                    TempData["ErrorMessage"] = result.ErrorMessages[0];
                    //success 0 redirect to Error controller 
                    return Json(new { success = 0 });
                }
                else if (!result.IsSuccess)
                {
                    //  Send "false"
                    return Json(new ServerResponse(result.IsSuccess) { Message = result.ErrorMessages[0] });
                }
                else
                {
                    //  Send "Success"
                    return Json(new ServerResponse(result.IsSuccess) { Message = "Your operation is successful!" });
                }
            }
            else
            {
                //  Send "false"
                return Json(new ServerResponse(false) { Message = "Id cost equals null!" });
            }
        }
        #endregion

        private ActionResult redirectToErrorPage(string errorTitle, string errorMsg)
        {
            TempData["ErrorTitle"] = errorTitle;
            TempData["ErrorMessage"] = errorMsg;
            return RedirectToAction("Error", "Finance");
        }
        //Error view Finance
        public IActionResult Error()
        {
            if (TempData["ErrorTitle"] != null)
            {
                ViewBag.ErrorTitle = TempData["ErrorTitle"];
            }
            else
            {
                ViewBag.ErrorTitle = "Page not found";
            }
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }*/
    }
}
