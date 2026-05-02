namespace TaskJect.Web.Models
{
    public class ProjectPermissionModel
    {
        public bool CanReadTask { get; set; }
        public bool CanCreateTask { get; set; }
        public bool CanEditTask { get; set; }
        public bool CanDeleteTask { get; set; }
        public bool CanAssignUsers { get; set; }
    }
}
