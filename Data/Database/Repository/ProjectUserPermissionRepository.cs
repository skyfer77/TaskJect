using AutoMapper;
using Domain.Database;
using Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Data.Database.Repository
{
    public class ProjectUserPermissionRepository : IProjectUserPermissionRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private IMapper _mapper;

        public ProjectUserPermissionRepository(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ProjectUserPermissionDto?> Retrieve(Guid projectId, string userId)
        {
            var permission = await _dbContext.ProjectUserPermission
                .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.UserId == userId);

            if (permission == null)
            {
                return null;
            }

            return _mapper.Map<ProjectUserPermissionDto>(permission);
        }

        public async Task<List<ProjectUserPermissionDto>> Retrieve(Guid projectId)
        {
            var permissions = await _dbContext.ProjectUserPermission
                .Where(p => p.ProjectId == projectId)
                .ToListAsync();

            return _mapper.Map<List<ProjectUserPermissionDto>>(permissions);
        }

        public async Task<bool> Insert(List<ProjectUserPermissionDto> permissionDtos)
        {
            var permissions = _mapper.Map<List<ProjectUserPermissionDto>, List<ProjectUserPermission>>(permissionDtos);
            try
            {
                var keys = permissions
                    .Select(p => new PermissionKey { ProjectId = p.ProjectId, UserId = p.UserId })
                    .ToList();

                var projectIds = keys.Select(k => k.ProjectId).Distinct().ToList();
                var userIds = keys.Select(k => k.UserId).Distinct().ToList();

                var existingKeys = await _dbContext.ProjectUserPermission
                    .Where(p => projectIds.Contains(p.ProjectId) && userIds.Contains(p.UserId))
                    .Select(p => new PermissionKey { ProjectId = p.ProjectId, UserId = p.UserId })
                    .ToListAsync();

                var existingKeySet = existingKeys.ToHashSet();

                var newPermissions = permissions
                    .Where(p => !existingKeySet.Contains(new PermissionKey { ProjectId = p.ProjectId, UserId = p.UserId }))
                    .ToList();

                if (!newPermissions.Any())
                {
                    return true;
                }

                await _dbContext.ProjectUserPermission.AddRangeAsync(newPermissions);

                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> InsertDefaultProjectsPermissionsForUsers(List<string> userIds, params Guid[] projectIds)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var allNewPermissions = new List<ProjectUserPermission>();

                foreach (var projectId in projectIds)
                {
                    var existingUserIds = await _dbContext.ProjectUserPermission
                        .Where(p => p.ProjectId == projectId && userIds.Contains(p.UserId))
                        .Select(p => p.UserId)
                        .ToListAsync();

                    var newPermissions = userIds
                        .Where(userId => !existingUserIds.Contains(userId))
                        .Select(userId => new ProjectUserPermission
                        {
                            ProjectId = projectId,
                            UserId = userId,
                            CanCreateTask = true,
                            CanEditTask = true,
                            CanDeleteTask = true,
                            CanAssignUsers = true
                        });

                    allNewPermissions.AddRange(newPermissions);
                }

                if (allNewPermissions.Any())
                {
                    await _dbContext.ProjectUserPermission.AddRangeAsync(allNewPermissions);
                    await _dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> InsertDefaultPermissionsForUsers(List<ProjectUserPermissionDto> permissionDtos)
        {
            if (permissionDtos == null || permissionDtos.Count == 0)
            {
                return false;
            }

            var projectId = permissionDtos.First().ProjectId;
            var userIds = permissionDtos.Select(x => x.UserId).Distinct().ToList();

            return await InsertDefaultProjectsPermissionsForUsers(userIds, projectId);
        }

        public async Task<bool> Update(List<ProjectUserPermissionDto> permissionDtos)
        {
            try
            {
                foreach (var dto in permissionDtos)
                {
                    var existing = await _dbContext.ProjectUserPermission
                        .FirstOrDefaultAsync(p => p.ProjectId == dto.ProjectId && p.UserId == dto.UserId);

                    if (existing != null)
                    {
                        existing.CanCreateTask = dto.CanCreateTask;
                        existing.CanEditTask = dto.CanEditTask;
                        existing.CanDeleteTask = dto.CanDeleteTask;
                        existing.CanAssignUsers = dto.CanAssignUsers;
                    }
                    else
                    {
                        var newEntity = _mapper.Map<ProjectUserPermission>(dto);
                        await _dbContext.ProjectUserPermission.AddAsync(newEntity);
                    }
                }

                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Delete(Guid projectId, string userId)
        {
            try
            {
                var permission = await _dbContext.ProjectUserPermission
                    .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.UserId.ToString() == userId);

                if (permission == null)
                {
                    return false;
                }

                _dbContext.ProjectUserPermission.Remove(permission);
                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Delete(Guid projectId)
        {
            try
            {
                var permissions = await _dbContext.ProjectUserPermission
                    .Where(p => p.ProjectId == projectId)
                    .ToListAsync();

                if (!permissions.Any())
                {
                    return false;
                }

                _dbContext.ProjectUserPermission.RemoveRange(permissions);
                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteRange(List<Guid> projectIds, List<string>? userIds = null)
        {
            try
            {
                if (projectIds == null || projectIds.Count == 0)
                {
                    return false;
                }

                var permissionsToDeleteQuery = _dbContext.ProjectUserPermission
                    .Where(p => projectIds.Contains(p.ProjectId));

                if (userIds != null && userIds.Count > 0)
                {
                    permissionsToDeleteQuery = permissionsToDeleteQuery
                        .Where(p => userIds.Contains(p.UserId));
                }

                var permissionsToDelete = await permissionsToDeleteQuery.ToListAsync();

                if (permissionsToDelete.Count == 0)
                {
                    return true;
                }

                _dbContext.ProjectUserPermission.RemoveRange(permissionsToDelete);
                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class PermissionKey : IEquatable<PermissionKey>
    {
        public Guid ProjectId { get; set; }
        public string UserId { get; set; }

        public override bool Equals(object obj)
            => Equals(obj as PermissionKey);

        public bool Equals(PermissionKey permissionKey)
            => permissionKey != null &&
                ProjectId.Equals(permissionKey.ProjectId) &&
                UserId == permissionKey.UserId;

        public override int GetHashCode()
            => HashCode.Combine(ProjectId, UserId);
    }
}
