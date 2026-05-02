using Data.DbContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Domain.Database;

namespace Data.Database.Repository
{
    internal class FullDeleteByTransaction
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public FullDeleteByTransaction(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<bool> DeleteOrganization(string organizationId)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var organizationGuid = Guid.Parse(organizationId);

                    var authorizedUsers = await _userManager.Users
                        .Where(u => u.OrganizationCode == organizationId)
                        .ToListAsync();

                    //Примусово розлогінюємо користувачів (оновлюється SecurityStamp)
                    foreach (var user in authorizedUsers)
                    {
                        await _userManager.UpdateSecurityStampAsync(user);
                    }

                    var files = await _dbContext.OrganizationFiles
                        .Where(f => f.OrganizationCode == organizationGuid)
                        .ToListAsync();

                    if (files.Any())
                    {
                        _dbContext.OrganizationFiles.RemoveRange(files);
                    }

                    var tasks = await _dbContext.Tasks
                        .Where(t => t.OrganizationCode == organizationId)
                        .ToListAsync();

                    if (tasks.Any())
                    {
                        _dbContext.Tasks.RemoveRange(tasks);
                    }

                    //ProjectUserPermission видаляються тригером в БД

                    var projects = await _dbContext.Projects
                        .Where(p => p.OrganizationCode == organizationId)
                        .ToListAsync();

                    if (projects.Any())
                    {
                        _dbContext.Projects.RemoveRange(projects);
                    }

                    var teams = await _dbContext.Teams
                        .Where(t => t.OrganizationCode == organizationId)
                        .ToListAsync();

                    if (teams.Any())
                    {
                        var teamIds = teams.Select(t => t.Id).ToList();

                        if (teamIds.Count > 0)
                        {
                            var parameters = teamIds.Select((id, i) => new SqlParameter($"@p{i}", id)).ToArray();
                            var inClause = string.Join(", ", parameters.Select(p => p.ParameterName));
                            var sql = $"DELETE FROM Membership WHERE TeamId IN ({inClause})";

                            await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
                        }
                    }

                    if (teams.Any())
                    {
                        _dbContext.Teams.RemoveRange(teams);
                    }

                    var tariffs = await _dbContext.TariffPlansHistories
                        .Where(t => t.OrganizationCode == organizationGuid)
                        .ToListAsync();

                    if (tariffs.Any())
                    {
                        _dbContext.TariffPlansHistories.RemoveRange(tariffs);
                    }

                    var users = await _dbContext.Users
                        .Where(u => u.OrganizationCode == organizationId)
                        .ToListAsync();

                    if (teams.Any())
                    {
                        _dbContext.Users.RemoveRange(users);
                    }

                    var appeals = await _dbContext.OrganizationAppeals
                        .Where(a => a.OrganizationCode == organizationGuid)
                        .ToListAsync();

                    if (appeals.Any())
                    {
                        _dbContext.OrganizationAppeals.RemoveRange(appeals);
                    }

                    var organization = await _dbContext.Organizations
                        .FirstOrDefaultAsync(o => o.OrganizationId == organizationGuid);

                    if (organization != null)
                    {
                        _dbContext.Organizations.Remove(organization);
                    }

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
