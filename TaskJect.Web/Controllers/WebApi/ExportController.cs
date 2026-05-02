using TaskJect.Web.Models;
using TaskJect.Web.Resources;
using TaskJect.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace TaskJect.Web.Controllers
{
	[Route("/api/export/")]
	public class ExportController : Controller
	{
		private readonly IExportService _exportService;
		private readonly ILogger<ExportController> _logger;
		private readonly IStringLocalizer<ErrorResources> _localizer;

		public ExportController(IExportService exportService, ILogger<ExportController> logger)
		{
			_exportService = exportService;
			_logger = logger;
		}

		[HttpPost("excel")]
		public IActionResult ExportToExcel([FromBody] ExportPayload payload)
		{
			try
			{
				payload.Culture = this.GetUserCulture();

				var bytes = _exportService.ExportToExcel(payload);
				var fileName = $"export_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

				return File(bytes,
					"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
					fileName);
			}
			catch (ArgumentException aex)
			{
				return BadRequest(aex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Execution error: {Message}", ex.Message);

				return Json(new ServerResponse(false)
				{
					Message = _localizer["SERVER_ERROR"]
				});
			}
		}

		[HttpPost("csv")]
		public IActionResult ExportToCsv([FromBody] ExportPayload payload)
		{
			try
			{
				payload.Culture = this.GetUserCulture();

				var bytes = _exportService.ExportToCsv(payload);
				var fileName = $"export_{DateTime.Now:yyyyMMdd_HHmm}.csv";

				return File(bytes, "text/csv", fileName);
			}
			catch (ArgumentException aex)
			{
				return BadRequest(aex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Execution error: {Message}", ex.Message);

				return Json(new ServerResponse(false)
				{
					Message = _localizer["SERVER_ERROR"]
				});
			}
		}
	}
}
