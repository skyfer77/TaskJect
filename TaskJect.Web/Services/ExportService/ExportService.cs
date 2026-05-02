using TaskJect.Web.Models;
using TaskJect.Web.Resources;
using ClosedXML.Excel;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Text;

namespace TaskJect.Web.Services
{
	public class ExportService : IExportService
	{
		private readonly IStringLocalizer<ErrorResources> _localizer;
		private readonly IStringLocalizer<SharedResources> _sharedLocalizer;

		public ExportService(IStringLocalizer<ErrorResources> localizer, IStringLocalizer<SharedResources> sharedLocalizer)
		{
			_localizer = localizer;
			_sharedLocalizer = sharedLocalizer;
		}

		public byte[] ExportToExcel(ExportPayload payload)
		{
			if (payload == null || payload.Rows == null || payload.Rows.Count == 0)
			{
				throw new ArgumentException(_localizer["NoDataToExport"]);
			}

			setCulture(payload.Culture);

			using var workbook = new XLWorkbook();
			var ws = workbook.Worksheets.Add("Export");

			int startRow = 1;

			var headerInfo = buildHeaderInfo(payload);

			if (!string.IsNullOrWhiteSpace(headerInfo))
			{
				ws.Cell(startRow, 1).Value = headerInfo;
				ws.Range(startRow, 1, startRow, payload.Headers.Count).Merge();
				ws.Cell(startRow, 1).Style.Font.Bold = true;
				startRow++;
			}

			for (int i = 0; i < payload.Headers.Count; i++)
			{
				ws.Cell(startRow, i + 1).Value = payload.Headers[i];
				ws.Cell(startRow, i + 1).Style.Font.Bold = true;
				ws.Cell(startRow, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
				ws.Cell(startRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
			}

			int dataStartRow = startRow + 1;
			for (int r = 0; r < payload.Rows.Count; r++)
			{
				var row = payload.Rows[r];
				for (int c = 0; c < row.Count; c++)
				{
					ws.Cell(dataStartRow + r, c + 1).Value = row[c];
					ws.Cell(dataStartRow + r, c + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
				}
			}

			ws.Columns().AdjustToContents();

			using var stream = new MemoryStream();
			workbook.SaveAs(stream);
			return stream.ToArray();
		}

		public byte[] ExportToCsv(ExportPayload payload)
		{
			if (payload == null || payload.Rows == null || payload.Rows.Count == 0)
			{
				throw new ArgumentException(_localizer["NoDataToExport"]);
			}

			setCulture(payload.Culture);

			var sb = new StringBuilder();

			var headerInfo = buildHeaderInfo(payload);

			if (!string.IsNullOrWhiteSpace(headerInfo))
			{
				sb.AppendLine(headerInfo);
			}

			if (payload.Headers != null && payload.Headers.Count > 0)
			{
				sb.AppendLine(string.Join(",", payload.Headers));
			}

			foreach (var row in payload.Rows)
			{
				sb.AppendLine(string.Join(",", row.Select(x => $"\"{x}\"")));
			}

			var bom = Encoding.UTF8.GetPreamble();
			return bom.Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
		}

		private string buildHeaderInfo(ExportPayload payload)
		{
			var header = "";

			if (!string.IsNullOrWhiteSpace(payload.User))
			{
				header += $"{_sharedLocalizer["User"]}: {payload.User}";
			}

			if (!string.IsNullOrWhiteSpace(payload.Period))
			{
				if (!string.IsNullOrEmpty(header))
				{
					header += " | ";
				}

				header += $"{_sharedLocalizer["Period"]}: {payload.Period}";
			}

			return header;
		}

		private void setCulture(string? culture)
		{
			if (string.IsNullOrEmpty(culture)) 
			{ 
				return; 
			}

			var cultureInfo = new CultureInfo(culture);
			CultureInfo.CurrentCulture = cultureInfo;
			CultureInfo.CurrentUICulture = cultureInfo;
		}
	}
}
