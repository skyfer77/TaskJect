namespace Domain.Database
{
    public class ProjectUserPermissionDto
    {
        public Guid ProjectId { get; set; }
        public string UserId { get; set; }
        public bool CanCreateTask { get; set; }
        public bool CanEditTask { get; set; }
        public bool CanDeleteTask { get; set; }
        public bool CanAssignUsers { get; set; }
    }
}
