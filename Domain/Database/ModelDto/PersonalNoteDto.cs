namespace Domain.Database
{
	public class PersonalNoteDto
	{
		public Guid Id { get; set; }
		public string UserId { get; set; } = null!;
		public string Title { get; set; }
		public string? Text { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
