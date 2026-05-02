using Domain.Database;
using TaskJect.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Data;

namespace TaskJect.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITariffPlanRepository _tariffPlanRepository;
        public HomeController(ILogger<HomeController> logger, ITariffPlanRepository tariffPlanRepository)
        {
            _logger = logger;
            _tariffPlanRepository = tariffPlanRepository;
        }

        public async Task<ActionResult> Index()
        {
            var tariffPlans = await _tariffPlanRepository.RetrievePublicPlansList();

            return View(tariffPlans);
        }

		public async Task<ActionResult> SaaSPirate()
		{
			var tariffPlans = await _tariffPlanRepository.RetrievePublicPlansList(SD.TariffPlanSource);

			return View(tariffPlans);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
