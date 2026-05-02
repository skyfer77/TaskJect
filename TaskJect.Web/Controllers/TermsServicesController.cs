using Microsoft.AspNetCore.Mvc;

namespace TaskJect.Web.Controllers
{
	public class TermsServicesController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
