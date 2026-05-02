using TaskJect.Web.Models;
using TaskJect.Web.Resources;
using TaskJect.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace TaskJect.Web.Controllers
{
    public class ContactUsController : Controller
    {
		private readonly IEmailRequestSender _emailRequestSender;
		private readonly IStringLocalizer<ErrorResources> _localizer;

		public ContactUsController(IEmailRequestSender emailRequestSender,
			IStringLocalizer<ErrorResources> localizer)
		{
			_emailRequestSender = emailRequestSender;
			_localizer = localizer;
		}

		public IActionResult Index()
        {
            return View();
        }

		[HttpPost]
		public async Task<IActionResult> Contact(EmailRequestModel request)
		{
			if (!ModelState.IsValid)
			{
				return Json(new ServerResponse(false) { Message = _localizer["AllFieldsMustFilled"] });
			}

			await _emailRequestSender.SendRequestEmailAsync(request);

			return Json(new ServerResponse(true) { Message = _localizer["TheLetterWasSentSuccessfully"] });
		}
	}
}
