using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Database
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = null!;
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool IsReviewed { get; set; }
        public DateTime Created { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
    }
}
