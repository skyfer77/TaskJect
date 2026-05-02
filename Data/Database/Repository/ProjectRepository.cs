using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Data.DbContexts;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.Extensions.Caching.Memory;
using Domain.Database;

namespace Data.Database.Repository
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private IMapper _mapper;
        private readonly IMemoryCache _cache;

        private const string CacheKey = "Projects";
        public ProjectRepository(ApplicationDbContext dbContext, IMapper mapper, IMemoryCache cache)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _cache = cache;
        }

        private async Task<List<Project>> getProjectsFromCache()
        {
            if (_cache == null || !_cache.TryGetValue(CacheKey, out List<Project> cachedProjects))
            {
                cachedProjects = await _dbContext.Projects.ToListAsync();
                _cache.Set(CacheKey, cachedProjects);
            }

            return cachedProjects;
        }

        private void clearCache()
        {
            _cache.Remove(CacheKey);
        }

        public async Task<bool> Delete(Guid id)
        {
            var project = await _dbContext.Projects.FirstOrDefaultAsync(x => x.Id == id);
            if (project == null)
            {
                return false;
            }
            var tasks = _dbContext.Tasks.Where(t => t.ProjectId == id);

            _dbContext.Tasks.RemoveRange(tasks);
            _dbContext.Projects.Remove(project);
            await _dbContext.SaveChangesAsync();

            clearCache();
            return true;
        }

        public async Task<bool> Insert(ProjectDto projectDto)
        {
            var project = _mapper.Map<ProjectDto, Project>(projectDto);
            try
            {
                await _dbContext.Projects.AddAsync(project);
                await _dbContext.SaveChangesAsync();

                clearCache();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<ProjectDto>> RetrieveByOrganization(string organizationCode)
        {
            var projects = await getProjectsFromCache();
            var filteredProjects = projects.Where(p => p.OrganizationCode == organizationCode).ToList();
            return _mapper.Map<List<ProjectDto>>(filteredProjects);
        }
        public async Task<List<ProjectDto>> RetrieveProjectsByTeam(Guid teamId)
        {
            var projects = await getProjectsFromCache();
            if (projects == null || !projects.Any(p => p.TeamId == teamId))
            {
                return null;
            }
            var filteredProjects = projects.Where(p => p.TeamId == teamId).ToList();
            return _mapper.Map<List<ProjectDto>>(filteredProjects);
        }

        public async Task<List<Guid>> RetrieveProjectIdsByTeam(Guid teamId)
        {
            var projects = await getProjectsFromCache();
            if (projects == null || !projects.Any(p => p.TeamId == teamId))
            {
                return null;
            }

            var filteredProjectIds = projects
                .Where(p => p.TeamId == teamId)
                .Select(p => p.Id)
                .ToList();

            return filteredProjectIds;
        }

        public async Task<ProjectDto> Retrieve(Guid id)
        {
            var projects = await getProjectsFromCache();
            var project = projects.FirstOrDefault(x => x.Id == id);
            return _mapper.Map<ProjectDto>(project);
        }

        public async Task<Dictionary<Guid, List<ProjectDto>>> RetrieveByTeamsIDs(List<Guid> teamIds)
        {
            if (!teamIds.Any())
            {
                return new Dictionary<Guid, List<ProjectDto>>();
            }
            var projects = await _dbContext.Projects
                .Where(p => p.TeamId.HasValue && teamIds.Contains(p.TeamId.Value))
                .ToListAsync();
            var filteredProjects = projects.Where(x => x.TeamId.HasValue && teamIds.Contains(x.TeamId.Value)).ToList();

            var dict = filteredProjects
                .GroupBy(x => x.TeamId.Value)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(x => _mapper.Map<ProjectDto>(x)).ToList());

            return dict;
        }
        public async Task<List<ProjectDto>> RetrieveByProjectIDs(List<Guid> projectIds)
        {
            if (projectIds == null || !projectIds.Any())
                return new List<ProjectDto>();

            var projects = new List<ProjectDto>();

            foreach (var projectId in projectIds)
            {
                var project = await _dbContext.Projects.FindAsync(projectId);
                if (project != null)
                {
                    projects.Add(_mapper.Map<ProjectDto>(project));
                }
            }

            return projects;
        }

        public async Task<Dictionary<Guid, string>> RetrieveNameProject(string organizationCode)
        {
            var projects = await getProjectsFromCache();
            var projectsName = projects
                .Where(p => p.OrganizationCode == organizationCode)
                .ToDictionary(p => p.Id, p => p.Title);

            return projectsName;
        }

        public async Task<Guid> RetrieveTeamId(Guid projectId)
        {
            var projects = await getProjectsFromCache();
            var teamId = projects
                .Where(p => p.Id == projectId)
                .Select(p => p.TeamId)
                .FirstOrDefault();

            return teamId.Value;
        }

        public async Task<string> RetrieveManagerId(Guid projectId)
        {
            var projects = await getProjectsFromCache();
            var managerId = projects
                .Where(p => p.Id == projectId)
                .Select(p => p.ManagerId)
                .FirstOrDefault();

            return managerId;
        }

        public async Task<bool> Update(ProjectDto projectDto)
        {
            var project = _mapper.Map<ProjectDto, Project>(projectDto);
            var existingProject = await _dbContext.Projects.FindAsync(project.Id);

            if (existingProject != null)
            {
                _mapper.Map(projectDto, existingProject);
                await _dbContext.SaveChangesAsync();

                clearCache();
                return true;
            }

            return false;
        }

        [Authorize(Roles = "Moderator, God, Admin")]
        public async Task<bool> DeleteByOrganization(string organizationId)
        {
            var projects = await _dbContext.Projects.Where(t => t.OrganizationCode == organizationId).ToListAsync();
            if (!projects.Any())
            {
                return true;
            }
            try
            {
                _dbContext.Projects.RemoveRange(projects);
                await _dbContext.SaveChangesAsync();

                clearCache();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region GitHub
        public async Task<GitHubInfo> FindGitHubInfo(Guid projectId)
        {
            var projectInfoGit = await _dbContext.Projects
                .Where(p => p.Id == projectId)
				.Select(p => new GitHubInfo() 
                { 
                    Owner = p.GitHubOwner, 
                    RepoName = p.GitHubRepoName 
                })
                .FirstOrDefaultAsync();

			return projectInfoGit;
		}

        public async Task<bool> UpdateGitHubInfo(ProjectDto projectDto)
        {
            var existingProject = await _dbContext.Projects.FindAsync(projectDto.ID);

            if (existingProject != null)
            {
                existingProject.GitHubOwner = projectDto.GitHubOwner;
                existingProject.GitHubRepoName = projectDto.GitHubRepoName;

                await _dbContext.SaveChangesAsync();

                clearCache();
                return true;
            }

            return false;
        }

        public async Task<Guid?> UpdateRepoName(GitHubUpdateRepo repo)
        {
            var existingProject = await _dbContext.Projects
                .Where(p => p.GitHubRepoName == repo.RepoName && p.GitHubOwner == repo.Owner)
                .FirstOrDefaultAsync();

            if (existingProject != null)
            {
                existingProject.GitHubRepoName = repo.NewRepoName;

                await _dbContext.SaveChangesAsync();

                clearCache();
                return existingProject.Id;
            }

            return null;
        }

        public async Task<List<Guid>> UpdateOwnerByOrganizationId(GitHubUpdateRepo repo)
        {
            var projects = await _dbContext.Projects
                .Where(p => p.OrganizationCode == repo.OrganizationId.ToString() && p.GitHubOwner == repo.Owner)
                .ToListAsync();

            var ids = projects.Select(p => p.Id).ToList();

            if (ids.Count == 0)
            {
                return ids;
            }

            foreach (var project in projects)
            {
                project.GitHubOwner = repo.NewOwner;
            }

            await _dbContext.SaveChangesAsync();

            return ids;
        }


        public async Task<Guid> RetrieveProjectId(string nameRepo, string owner)
        {
            var projectId = await _dbContext.Projects
                .Where(p => p.GitHubRepoName == nameRepo && p.GitHubOwner == owner)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            return projectId;
        }

        public async Task<string?> GetCurrentRepoFullName(Guid projectId)
        {
            var repo = await _dbContext.Projects
                .Where(p => p.Id == projectId)
                .Select(p => new { p.GitHubOwner, p.GitHubRepoName })
                .FirstOrDefaultAsync();

            if (repo == null || string.IsNullOrEmpty(repo.GitHubOwner) || string.IsNullOrEmpty(repo.GitHubRepoName))
            {
                return null;
            }

            return $"{repo.GitHubOwner}/{repo.GitHubRepoName}";
        }

        #endregion
    }
}
