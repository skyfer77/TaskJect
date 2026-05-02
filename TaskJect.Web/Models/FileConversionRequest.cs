namespace TaskJect.Web.Models
{
	public class FileConversionRequest
	{
		public IFormFile File { get; set; }
		public Guid? TaskId { get; set; }
		public Guid ProjectId { get; set; }
		public string OrganizationCode { get; set; }
	}
}
