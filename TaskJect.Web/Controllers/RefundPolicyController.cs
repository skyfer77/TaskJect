using Microsoft.AspNetCore.Mvc;

namespace TaskJect.Web.Controllers
{
    public class RefundPolicyController : Controller
    {

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
