namespace Domain.Database
{
	public class PersonalTodoTaskDto
	{
		public Guid Id { get; set; }
		public Guid TodoId { get; set; }
		public string Text { get; set; }
		public bool IsDone { get; set; }
		public int SortOrder { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
