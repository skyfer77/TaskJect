using TaskJect.Web.Enums;
using Domain.Database;

namespace TaskJect.Web.Models
{
    public class CreateTaskModel
    {
        public Guid ProjectId { get; set; }
        public TaskStatusView Status { get; set; }  
        public List<ApplicationUserLiteDto> Users { get; set; }
        public string CreatedByUserId { get; set; }
        public string Creator { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<IFormFile> Files { get; set; }
        public GitHubViewModel GitHubViewModel { get; set; }
    }
}