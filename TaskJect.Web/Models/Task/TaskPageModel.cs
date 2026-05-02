using Domain.Database;

namespace TaskJect.Web.Models
{
    public class TaskPageModel
    {
        public List<TaskView> Tasks { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; }
        public List<ApplicationUserLiteDto> Users { get; set; }
    }
}
