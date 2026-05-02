using Microsoft.AspNetCore.Mvc;

namespace TaskJect.Web.Controllers
{
    public class PrivacyPolicyController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
