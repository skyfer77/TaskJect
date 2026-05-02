namespace Domain.Database
{
    public class TaskDetailsRequest
    {
        public string UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
		public string OrganizationCode { get; set; }
	}
}
