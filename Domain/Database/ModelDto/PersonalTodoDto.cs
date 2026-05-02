namespace Domain.Database
{
	public class PersonalTodoDto
	{
		public Guid Id { get; set; }
		public string UserId { get; set; } = null!;
		public string Title { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }

		public List<PersonalTodoTaskDto> Tasks { get; set; } = new List<PersonalTodoTaskDto>();
	}
}
