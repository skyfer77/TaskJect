namespace Domain.Database
{
    public interface IProjectUserPermissionRepository
    {
        Task<ProjectUserPermissionDto?> Retrieve(Guid projectId, string userId);
        Task<List<ProjectUserPermissionDto>> Retrieve(Guid projectId);
        Task<bool> Insert(List<ProjectUserPermissionDto> permissionDtos);
        Task<bool> InsertDefaultProjectsPermissionsForUsers(List<string> userIds, params Guid[] projectIds);
        Task<bool> InsertDefaultPermissionsForUsers(List<ProjectUserPermissionDto> permissionDtos);
        Task<bool> Update(List<ProjectUserPermissionDto> permissionDtos);
        Task<bool> Delete(Guid projectId, string userId);
        Task<bool> Delete(Guid projectId);
        Task<bool> DeleteRange(List<Guid> projectIds, List<string>? userIds = null);

    }
}
