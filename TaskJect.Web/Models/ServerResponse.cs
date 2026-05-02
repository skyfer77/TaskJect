using TaskJect.Web.Enums;

namespace TaskJect.Web.Models
{
    public class ServerResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
        public Guid? ProjectId { get; set;}
        public Guid? TaskId { get; set;}
		public List<Guid>? TaskIds { get; set; }
		public TaskStatusView TaskStatus { get; set; }
        public string? Html { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? RedirectUrl { get; set; }

		public ServerResponse(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }
    }
}
