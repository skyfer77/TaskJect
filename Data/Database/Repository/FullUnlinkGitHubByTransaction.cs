using Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Data.Database.Repository
{
    public class FullUnlinkGitHubByTransaction
    {
        private readonly ApplicationDbContext _dbContext;

        public FullUnlinkGitHubByTransaction(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> UnlinkGitHubOrganization(Guid organizationId)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var installationId = await _dbContext.Organizations
                        .Where(o => o.OrganizationId == organizationId)
                        .Select(o => o.GitHubInstallationId)
                        .FirstOrDefaultAsync();

                    if (installationId == null)
                    {
                        return false;
                    }

                    var organizationCode = organizationId.ToString();

                    var tasks = await _dbContext.Tasks
                        .Where(t => t.OrganizationCode == organizationCode && t.IsGitHubIntegration)
                        .ToListAsync();

                    foreach (var t in tasks)
                    {
                        t.IsGitHubIntegration = false;
                        t.GitHubBranch = null;
                        t.GitHubOwner = null;
                        t.GitHubRepoName = null;
                        t.GitHubIssueNumber = null;
                    }

                    var projects = await _dbContext.Projects
                        .Where(p => p.OrganizationCode == organizationCode && p.GitHubRepoName != null)
                        .ToListAsync();

                    foreach (var p in projects)
                    {
                        p.GitHubOwner = null;
                        p.GitHubRepoName = null;
                    }

                    var organization = await _dbContext.Organizations.FirstAsync(o => o.OrganizationId == organizationId);
                    organization.GitHubInstallationId = null;

                    await _dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
        }

        public async Task<bool> UnlinkGitHubProject(Guid projectId)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var project = await _dbContext.Projects
                        .FirstOrDefaultAsync(p => p.Id == projectId);

                    if (project == null)
                    {
                        return false;
                    }

                    var organizationCode = project.OrganizationCode;

                    var tasks = await _dbContext.Tasks
                        .Where(t => t.ProjectId == projectId && t.IsGitHubIntegration)
                        .ToListAsync();

                    foreach (var t in tasks)
                    {
                        t.IsGitHubIntegration = false;
                        t.GitHubBranch = null;
                        t.GitHubOwner = null;
                        t.GitHubRepoName = null;
                        t.GitHubIssueNumber = null;
                    }

                    project.GitHubOwner = null;
                    project.GitHubRepoName = null;

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
        }
    }
}
