using Domain.Database;
namespace TaskJect.Web.Models
{
    	public class ProfileViewModel
	{
		public ApplicationUserLiteDto User { get; set; }
		public List<ProjectDto> Projects { get; set; }
        public Dictionary<TaskDto, string> TasksWithProjectNames { get; set; }
        public string ThisUserId { get; set; }
		public string PersonalTelegramLink { get; set; }
    }
}
