namespace TaskJect.Web.Models
{
	public class ExportPayload
	{
		public List<string>? Headers { get; set; }
		public List<List<string>>? Rows { get; set; }
		public string? ExportType { get; set; }
		public string? Period { get; set; }
		public string? User { get; set; }
		public string? Culture { get; set; }
	}
}
