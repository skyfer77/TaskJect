using Domain.Database;
namespace TaskJect.Web.Models
{
	public class TodoPageViewModel
	{
		public IEnumerable<PersonalTodoDto> PersonalTodos { get; set; }
		public IEnumerable<TaskDto> Tasks { get; set; }
	}
}
