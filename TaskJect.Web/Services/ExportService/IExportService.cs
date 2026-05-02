using TaskJect.Web.Models;

namespace TaskJect.Web.Services
{
	public interface IExportService
	{
		byte[] ExportToExcel(ExportPayload payload);
		byte[] ExportToCsv(ExportPayload payload);
	}
}
