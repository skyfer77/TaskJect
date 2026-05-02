using Domain.Database;
using Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace TaskJect.Web.Services
{
    public class ProjectUserPermissionInitializer : IProjectUserPermissionInitializer
    {
        private readonly IProjectUserPermissionRepository _projectUserPermissionRepository;
        private readonly ApplicationDbContext _dbContext;
        public ProjectUserPermissionInitializer(IProjectUserPermissionRepository projectUserPermissionRepository,
            ApplicationDbContext dbContext)
        {
            _projectUserPermissionRepository = projectUserPermissionRepository;
            _dbContext = dbContext;
        }
        public async System.Threading.Tasks.Task InitializeAsync()
        {
            var projects = await _dbContext.Projects
                .Select(p => new ProjectTeam 
                {
                    ProjectId = p.Id, 
                    TeamId = p.TeamId.Value 
                })
                .ToListAsync();

            var teamIds = projects.Select(p => p.TeamId).Distinct().ToList();

            var memberships = await _dbContext.Memberships
                .Where(m => teamIds.Contains(m.TeamId))
                .ToListAsync();

            var membershipsByTeam = memberships
                .GroupBy(m => m.TeamId)
                .ToDictionary(g => g.Key, g => g.Select(m => m.UserId).ToList());

            foreach (var project in projects)
            {
                if (!membershipsByTeam.TryGetValue(project.TeamId, out var userIds))
                {
                    continue;
                }

                await _projectUserPermissionRepository.InsertDefaultProjectsPermissionsForUsers(userIds, project.ProjectId);
            }
        }
    }

    public class ProjectTeam
    {
        public Guid ProjectId { get; set; }
        public Guid TeamId { get; set; }
    }
}
