using Domain.Database;

namespace TaskJect.Web.Models
{
    public class EditTaskModel
    {
        public TaskView Task { get; set; }
        public List<ApplicationUserLiteDto> Users { get; set; }
        public string Creator {get; set;}
        public List<LightOrganizationFiles>? OrganizationFiles { get; set;}
    }
}
