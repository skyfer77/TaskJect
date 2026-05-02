using Microsoft.AspNetCore.Mvc;

namespace TaskJect.Web.Controllers
{
	public class FAQController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
