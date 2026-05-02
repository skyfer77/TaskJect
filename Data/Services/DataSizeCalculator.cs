using Domain.Database;
using Domain.IServices;
using Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Data.Services
{
    internal class DataSizeCalculator : IDataSizeCalculator
    {
        private readonly ApplicationDbContext _context;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IOrganizationFilesRepository _organizationFilesRepository;

        public DataSizeCalculator(ApplicationDbContext context, IApplicationUserRepository applicationUserRepository, IOrganizationFilesRepository organizationFilesRepository)
        {
            _context = context;
            _applicationUserRepository = applicationUserRepository;
            _organizationFilesRepository = organizationFilesRepository;
        }

        public async Task<long> CalculateOrganizationDataSize(string organizationCode)
        {
            long totalSizeBytes = 0;
            var orgGuid = Guid.Parse(organizationCode);

            // Підрахунок для таблиці Organization
            var organization = await _context.Organizations
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrganizationId == orgGuid);

            if (organization != null)
            {
                totalSizeBytes += (long)((
                    ((organization.Name?.Length ?? 0) +
                    (organization.Email?.Length ?? 0) +
                    (organization.PhoneNumber?.Length ?? 0))) * sizeof(char) +
                    16 + // OrganizationId (GUID завжди 16 байт)
                    (organization.Picture?.Length ?? 0) + //TODO: Додати підрахунок зайнятої пам'яті для історії тарифів
                    8 + // RegistrationDate
                    (organization.LockoutEnabled.HasValue ? sizeof(bool) : 0) +
                    (organization.LockoutEnd.HasValue ? 8 : 0) +
                    8 //UsedStorageSpace
                    );
            }

            // Projects — SUM довжин напряму в SQL
            var projects = await _context.Projects
            .Where(p => p.OrganizationCode == organizationCode)
            .ToListAsync();

            var projectIds = projects.Select(p => p.Id).ToList(); // ID потрібні далі для ProjectPermission

            var projectsSize = projects.Sum(p =>
                  (
                      (p.Title?.Length ?? 0) +
                      (p.Description?.Length ?? 0) +
                      (p.ShortDescription?.Length ?? 0) +
                      (p.ManagerId?.Length ?? 0) +
                      (p.Client?.Length ?? 0) +
                      (p.File?.Length ?? 0) +
                      (p.OrganizationCode?.Length ?? 0)
                  ) * sizeof(char) +
                  16 + // ProjectId (GUID)
                  (p.StartDate.HasValue ? 8 : 0) +
                  (p.EndDate.HasValue ? 8 : 0) +
                  (p.DateEdit.HasValue ? 8 : 0) +
                  (p.DateAdd.HasValue ? 8 : 0) +
                  (p.TeamId.HasValue ? 16 : 0) +
                  (p.Status.HasValue ? sizeof(int) : 0) +
                  (p.Priority.HasValue ? sizeof(int) : 0)
              );

            totalSizeBytes += projectsSize;

            // Tasks
            var tasksSize = await _context.Tasks
                    .Where(t => t.OrganizationCode == organizationCode.ToUpper())
                    .Select(t =>
                        (
                            t.Title.Length +
                            (t.Description != null ? t.Description.Length : 0) +
                            (t.PerformanceNote != null ? t.PerformanceNote.Length : 0) +
                            (t.AssigneeId != null ? t.AssigneeId.Length : 0) +
                            t.OrganizationCode.Length +
                            (t.CreatedByUserId != null ? t.CreatedByUserId.Length : 0)
                        ) * sizeof(char) +
                        16 +
                        16 +
                        (t.ComplitedDate.HasValue ? 8 : 0) +
                        (t.StartDate.HasValue ? 8 : 0) +
                        (t.EndDate.HasValue ? 8 : 0) +
                        (t.ReviewDate.HasValue ? 8 : 0) +
                        (t.DateAdd.HasValue ? 8 : 0) +
                        (t.DateEdit.HasValue ? 8 : 0) +
                        (t.ActualHours.HasValue ? sizeof(int) : 0) +
                        (t.ActualMinutes.HasValue ? sizeof(int) : 0) +
                        (t.Status != null ? sizeof(int) : 0) +
                        (t.Priority != null ? sizeof(int) : 0) +
                        sizeof(int) +
                        sizeof(bool)
                    )
                    .SumAsync();

            totalSizeBytes += tasksSize;

            // Teams
            var teams = await _context.Teams
                .Where(t => t.OrganizationCode == organizationCode)
                .Select(t => new { t.Id, t.Name, t.OrganizationCode })
                .ToListAsync(); // ID потрібні далі для Memberships

            var teamsSize = teams.Sum(t =>
                ((t.Name?.Length ?? 0) + (t.OrganizationCode?.Length ?? 0)) * sizeof(char) + 16);

            totalSizeBytes += teamsSize;

            // Memberships
            var teamIds = teams.Select(t => t.Id).ToList();

            var membershipsSize = await _context.Memberships
                .Where(m => teamIds.Contains(m.TeamId))
                .Select(m =>
                    16 + 16 + (m.UserId.Length) * sizeof(char))
                .SumAsync();

            totalSizeBytes += membershipsSize;

            // OrganizationFiles
            var organizationFilesSize = await _context.OrganizationFiles
                .Where(f => f.OrganizationCode == orgGuid)
                .Select(f =>
                    // Size (фактичний розмір файлу)
                    (long?)f.Size
                    // + Size поля (long) як тип — 8 байт
                    + 8
                    // + GUIDs (4 x 16 байт)
                    + 16 + 16 + 16 + (f.TaskId != null ? 16 : 0)
                    // + DateTime
                    + 8
                    // + орієнтовна довжина string
                    + (f.FileName != null ? f.FileName.Length * 2 : 0)
                    + (f.ContentType != null ? f.ContentType.Length * 2 : 0)
                )
                .SumAsync() ?? 0;

            totalSizeBytes += organizationFilesSize;

            // Users
            var userSizeBytes = await _applicationUserRepository.GetUsedStorageUsers(organizationCode);
            totalSizeBytes += userSizeBytes;

            return totalSizeBytes;
        }

        public async Task<List<TaskWithFiles>> GetTasksWithFiles(string organizationCode)
        {
            var tasks = await _context.Tasks
                .Where(t => t.OrganizationCode == organizationCode)
                .OrderBy(t => t.DateAdd) 
                .ToListAsync();

            var taskIds = tasks.Select(t => t.Id).ToList();

            var files = await _organizationFilesRepository.RetrieveLightTaskFiles(taskIds);

            var result = tasks.Select(t =>
            {
                var taskFiles = files.Where(f => f.TaskId == t.Id).ToList();

                long filesSize = taskFiles.Sum(f =>
                      ((long?)f.Size ?? 0)        
                    + 8
                    + 16 + 16 + 16 + (f.TaskId != null ? 16 : 0)
                    + 8
                    + (f.FileName != null ? f.FileName.Length * 2 : 0)
                    + (f.ContentType != null ? f.ContentType.Length * 2 : 0)
                );

                long taskSize = EstimateTaskSize(t);

                return new TaskWithFiles
                {
                    TaskId = t.Id,
                    FilesIds = taskFiles.Select(f => f.Id).ToList(),
                    UsedStorageSpace = taskSize + filesSize
                };
            })
            .ToList();

            return result;
        }

        public async Task<Dictionary<Guid, long>> GetProjectFiles(string organizationCode)
        {
            var projectFiles = await _context.OrganizationFiles
                .Where(f => f.TaskId == null && f.OrganizationCode == Guid.Parse(organizationCode))
                .OrderBy(f => f.DateUploaded)
                .ToListAsync();

            return projectFiles.ToDictionary(f => f.Id, f =>
                ((long?)f.Size ?? 0)
                + 8
                + 16 + 16 + 16 + (f.TaskId != null ? 16 : 0)
                + 8
                + (f.FileName != null ? f.FileName.Length * 2 : 0)
                + (f.ContentType != null ? f.ContentType.Length * 2 : 0)
            );
        }

        private long EstimateTaskSize(Domain.Database.Task t)
        {
            long size = 0;

            size += (t.Title.Length +
                            (t.Description != null ? t.Description.Length : 0) +
                            (t.PerformanceNote != null ? t.PerformanceNote.Length : 0) +
                            (t.AssigneeId != null ? t.AssigneeId.Length : 0) +
                            t.OrganizationCode.Length +
                            (t.CreatedByUserId != null ? t.CreatedByUserId.Length : 0)
                     ) * sizeof(char) +
                        16 +
                        16 +
                        (t.ComplitedDate.HasValue ? 8 : 0) +
                        (t.StartDate.HasValue ? 8 : 0) +
                        (t.EndDate.HasValue ? 8 : 0) +
                        (t.ReviewDate.HasValue ? 8 : 0) +
                        (t.DateAdd.HasValue ? 8 : 0) +
                        (t.DateEdit.HasValue ? 8 : 0) +
                        (t.ActualHours.HasValue ? sizeof(int) : 0) +
                        (t.ActualMinutes.HasValue ? sizeof(int) : 0) +
                        (t.Status != null ? sizeof(int) : 0) +
                        (t.Priority != null ? sizeof(int) : 0) +
                        sizeof(int) +
                        sizeof(bool);

            return size;
        }

    }
}
