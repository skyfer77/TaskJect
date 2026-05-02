using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace TaskJect.Web.Controllers
{
	[Route("sitemap.xml")]
	public class SitemapController : Controller
	{
		private readonly IConfiguration _configuration;

		public SitemapController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		[HttpGet]
		public IActionResult Index()
		{
			// визначаємо namespace для sitemap
			XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

			string domain = _configuration["Domain"];

			var urls = new List<(string loc, DateTime lastMod, string changefreq, string priority)>
			{
				(domain, DateTime.UtcNow, "daily", "1.0"),
				($"{domain}Account/Login", DateTime.UtcNow, "monthly", "0.5"),
				($"{domain}Account/ForgotPassword", DateTime.UtcNow, "monthly", "0.3"),
				($"{domain}ContactUs", DateTime.UtcNow, "monthly", "0.7"),
				($"{domain}PrivacyPolicy", DateTime.UtcNow, "yearly", "0.3"),
				($"{domain}RefundPolicy", DateTime.UtcNow, "yearly", "0.3"),
			};

			var xml = new XDocument(
				new XDeclaration("1.0", "utf-8", "yes"),
				new XElement(ns + "urlset",
					urls.Select(url =>
						new XElement(ns + "url",
							new XElement(ns + "loc", url.loc),
							new XElement(ns + "lastmod", url.lastMod.ToString("yyyy-MM-dd")),
							new XElement(ns + "changefreq", url.changefreq),
							new XElement(ns + "priority", url.priority)
						)
					)
				)
			);

			return Content(xml.ToString(), "application/xml");
		}
	}
}
