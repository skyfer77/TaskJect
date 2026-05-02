using Domain.Database;
namespace TaskJect.Web.Models
{
    public class AnalysisProjectModel
    {
        public ProjectDto Project { get; set; }
        public TeamDto Team { get; set; }
        public List<ApplicationUserLiteDto> Users { get; set; }
        public List<TaskDto> Tasks { get; set; }
    }
}
