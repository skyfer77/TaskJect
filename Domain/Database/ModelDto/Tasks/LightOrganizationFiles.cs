namespace Domain.Database
{
	public class LightOrganizationFiles
	{
		public Guid Id { get; set; }
		public string FileName { get; set; }
		public string ContentType { get; set; }
		public long Size { get; set; }

		public Guid? ProjectId { get; set; }
		public Guid? TaskId { get; set; }
		public Guid OrganizationCode { get; set; }
		public DateTime DateUploaded { get; set; }
	}
}
