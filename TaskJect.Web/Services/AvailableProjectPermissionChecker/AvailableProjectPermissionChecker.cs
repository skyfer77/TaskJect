using Domain.Database;
using TaskJect.Web.Models;

namespace TaskJect.Web.Services
{
    public class AvailableProjectPermissionChecker : IAvailableProjectPermissionChecker
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IProjectUserPermissionRepository _projectUserPermissionRepository;

        public AvailableProjectPermissionChecker(
            IProjectRepository projectRepository, 
            IMembershipRepository membershipRepository,
            IProjectUserPermissionRepository projectUserPermissionRepository)
        {
            _projectRepository = projectRepository;
            _membershipRepository = membershipRepository;
            _projectUserPermissionRepository = projectUserPermissionRepository;
        }

        public async Task<ProjectPermissionModel> Check(Guid projectId, string userId)
        {
            var permission = await _projectUserPermissionRepository.Retrieve(projectId, userId);
            if (permission == null) 
            {
                return new ProjectPermissionModel()
                {
                    CanReadTask = false
                };
            }

            var permissionUser = new ProjectPermissionModel()
            {
                CanCreateTask = permission.CanCreateTask,
                CanEditTask = permission.CanEditTask,
                CanDeleteTask = permission.CanDeleteTask,
                CanAssignUsers = permission.CanAssignUsers,
            };

            var teamId = await _projectRepository.RetrieveTeamId(projectId);

            var result = await _membershipRepository.IsUserOnTeam(teamId, userId);
            if (result)
            {
                permissionUser.CanReadTask = true;
            }
            else 
            {
                permissionUser.CanReadTask = false;
            }

            return permissionUser;
        }
    }
}
