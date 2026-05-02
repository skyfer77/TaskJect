using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Data.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Domain.IServices;
using Domain.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

namespace Data.Database.Repository
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private IMapper _mapper;
        private readonly IOrganizationStorageChecker _organizationStorageChecker;
        private readonly UserManager<ApplicationUser> _userManager;
        public OrganizationRepository(ApplicationDbContext applicationDbContext, IMapper mapper,
            IOrganizationStorageChecker organizationStorageChecker, UserManager<ApplicationUser> userManager)
        {
            _applicationDbContext = applicationDbContext;
            _mapper = mapper;
            _organizationStorageChecker = organizationStorageChecker;
            _userManager = userManager;
        }

        public async Task<IEnumerable<OrganizationDto>> Retrieve()
        {
            var organizations = await _applicationDbContext.Organizations.ToListAsync();
            return _mapper.Map<List<OrganizationDto>>(organizations);
        }

        public async Task<OrganizationDto> GetOrganizationById(Guid organizetionId)
        {
            var organistation = await _applicationDbContext.Organizations.FirstOrDefaultAsync(x => x.OrganizationId == organizetionId);
            return _mapper.Map<OrganizationDto>(organistation);
        }
        public async Task<List<OrganizationDto>> GetOrganizationsByIds(List<Guid> organizationIds)
        {
            var organizations = await _applicationDbContext.Organizations.Where(x => organizationIds.Contains(x.OrganizationId)).ToListAsync();

            return _mapper.Map<List<OrganizationDto>>(organizations);
        }

        public async Task<OrganizationDto> GetOrganizationByName(string organizetionName)
        {
            var organistation = await _applicationDbContext.Organizations.FirstOrDefaultAsync(x => x.Name == organizetionName);
            return _mapper.Map<OrganizationDto>(organistation);
        }

        public async Task<bool> Insert(OrganizationDto organizationDto)
        {
            var organization = _mapper.Map<Organization>(organizationDto);
            organization.RegistrationDate = DateTime.Now;
            if (organization.OrganizationId == Guid.Empty)
            {
                organization.OrganizationId = Guid.NewGuid();
            }
            try
            {
                var existingOrganization = await _applicationDbContext.Organizations
                    .FirstOrDefaultAsync(o => o.Name == organization.Name);

                if (existingOrganization != null)
                {
                    return false;
                }

                _applicationDbContext.Organizations.Add(organization);

                await _applicationDbContext.SaveChangesAsync();
                _organizationStorageChecker.ClearCache();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Update(OrganizationDto organizationDto)
        {
            var organization = await _applicationDbContext.Organizations
                .FirstOrDefaultAsync(o => o.OrganizationId == organizationDto.OrganizationId);

            if (organization != null)
            {
                _mapper.Map(organizationDto, organization);

                _applicationDbContext.Organizations.Update(organization);

                await _applicationDbContext.SaveChangesAsync();
                _organizationStorageChecker.ClearCache();

                return true;
            }

            return false;
        }

        public async Task<bool> LockoutUnlockout(bool isLockout, Guid organizationId)
        {
            var organization = await _applicationDbContext.Organizations.FirstOrDefaultAsync(x => x.OrganizationId == organizationId);
            if (organization.OrganizationId != null)
            {
                organization.LockoutEnabled = isLockout;
                if (isLockout)
                {
                    organization.LockoutEnd = DateTime.MaxValue;
                }
                else
                {
                    organization.LockoutEnd = null;
                }
                await _applicationDbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }
        public async Task<bool> DeleteOrganization(string organizationId)
        {
            using (var transaction = await _applicationDbContext.Database.BeginTransactionAsync())
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

                    if (authorizedUsers.Any())
                    {
                        _applicationDbContext.Users.RemoveRange(authorizedUsers);
                    }

                    var files = await _applicationDbContext.OrganizationFiles
                        .Where(f => f.OrganizationCode == organizationGuid)
                        .ToListAsync();

                    if (files.Any())
                    {
                        _applicationDbContext.OrganizationFiles.RemoveRange(files);
                    }

                    var tasks = await _applicationDbContext.Tasks
                        .Where(t => t.OrganizationCode == organizationId)
                        .ToListAsync();

                    if (tasks.Any())
                    {
                        _applicationDbContext.Tasks.RemoveRange(tasks);
                    }

                    //ProjectUserPermission видаляються тригером в БД

                    var projects = await _applicationDbContext.Projects
                        .Where(p => p.OrganizationCode == organizationId)
                        .ToListAsync();

                    if (projects.Any())
                    {
                        _applicationDbContext.Projects.RemoveRange(projects);
                    }

                    var teams = await _applicationDbContext.Teams
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

                            await _applicationDbContext.Database.ExecuteSqlRawAsync(sql, parameters);
                        }
                    }

                    if (teams.Any())
                    {
                        _applicationDbContext.Teams.RemoveRange(teams);
                    }

                    var tariffs = await _applicationDbContext.TariffPlansHistories
                        .Where(t => t.OrganizationCode == organizationGuid)
                        .ToListAsync();

                    if (tariffs.Any())
                    {
                        _applicationDbContext.TariffPlansHistories.RemoveRange(tariffs);
                    }

                    var users = await _applicationDbContext.Users
                        .Where(u => u.OrganizationCode == organizationId)
                        .ToListAsync();

                    if (teams.Any())
                    {
                        _applicationDbContext.Users.RemoveRange(users);
                    }

                    var appeals = await _applicationDbContext.OrganizationAppeals
                        .Where(a => a.OrganizationCode == organizationGuid)
                        .ToListAsync();

                    if (appeals.Any())
                    {
                        _applicationDbContext.OrganizationAppeals.RemoveRange(appeals);
                    }

                    var organization = await _applicationDbContext.Organizations
                        .FirstOrDefaultAsync(o => o.OrganizationId == organizationGuid);

                    if (organization != null)
                    {
                        _applicationDbContext.Organizations.Remove(organization);
                    }

                    await _applicationDbContext.SaveChangesAsync();
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

        [Authorize(Roles = "Moderator, Admin, God")]
        public async Task<bool> Delete(Guid organizationId)
        {
            var organization = await _applicationDbContext.Organizations.FirstOrDefaultAsync(x => x.OrganizationId == organizationId);
            if (organization == null)
            {
                return false;
            }
            _applicationDbContext.Organizations.Remove(organization);
            await _applicationDbContext.SaveChangesAsync();
            _organizationStorageChecker.ClearCache();
            return true;
        }

        #region GitHub
        public async Task<Guid?> GetIdByInstallationId(long installationId)
        {
            var orgId = await _applicationDbContext.Organizations
                .Where(p => p.GitHubInstallationId == installationId)
                .Select(p => p.OrganizationId)
                .FirstOrDefaultAsync();

            return orgId;
        }
        public async Task<long?> FindGitHubInstallationId(Guid organizetionId)
        {
            var installationId = await _applicationDbContext.Organizations
                .Where(o => o.OrganizationId == organizetionId)
                .Select(o => o.GitHubInstallationId)
                .FirstOrDefaultAsync();

            return installationId;
        }

        public async Task<bool> SetGitHubInstallationId(Guid organizetionId, long installationId)
        {
            var existingOrganization = await _applicationDbContext.Organizations.FindAsync(organizetionId);

            if (existingOrganization != null)
            {
                existingOrganization.GitHubInstallationId = installationId;
                await _applicationDbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }
        #endregion
    }
}
