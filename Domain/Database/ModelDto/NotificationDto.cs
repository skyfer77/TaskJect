namespace Domain.Database
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = null!;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool IsReviewed { get; set; }
        public DateTime Created { get; set; }
    }
}
