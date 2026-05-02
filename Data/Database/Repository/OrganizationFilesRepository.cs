using AutoMapper;
using Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Domain.Database;

namespace Data.Database.Repository
{
    public class OrganizationFilesRepository : IOrganizationFilesRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public OrganizationFilesRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<OrganizationFilesDto> Retrieve(Guid Id)
        {
            var entity = await _context.OrganizationFiles.FindAsync(Id);
            return _mapper.Map<OrganizationFilesDto>(entity);
        }

        public async Task<IEnumerable<LightOrganizationFiles>> RetrieveLightTaskFile(Guid taskId)
        {
            return await _context.OrganizationFiles
                .Where(f => f.TaskId == taskId)
                .Select(f => new LightOrganizationFiles
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    ContentType = f.ContentType,
                    Size = f.Size,
                    ProjectId = f.ProjectId,
                    TaskId = f.TaskId,
                    OrganizationCode = f.OrganizationCode,
                    DateUploaded = f.DateUploaded
                })
                .ToListAsync();
        }
        public async Task<IEnumerable<LightOrganizationFiles>> RetrieveLightTaskFiles(IEnumerable<Guid> taskIds)
        {
            if (taskIds == null || !taskIds.Any())
            {
                return Enumerable.Empty<LightOrganizationFiles>();
            }

            var result = await _context.OrganizationFiles
                .AsNoTracking()
                .Where(f => f.TaskId != null && taskIds.Contains(f.TaskId.Value))
                .Select(f => new LightOrganizationFiles
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    ContentType = f.ContentType,
                    Size = f.Size,
                    ProjectId = f.ProjectId,
                    TaskId = f.TaskId,
                    OrganizationCode = f.OrganizationCode,
                    DateUploaded = f.DateUploaded
                })
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<LightOrganizationFiles>> RetrieveLightProjectFile(Guid projectId)
        {
            //У файлів проекту нема Task Id
            //У тасок проекта є Project Id і Task Id
            return await _context.OrganizationFiles
                .Where(f => f.ProjectId == projectId && f.TaskId == null)
                .Select(f => new LightOrganizationFiles
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    ContentType = f.ContentType,
                    Size = f.Size,
                    ProjectId = f.ProjectId,
                    TaskId = f.TaskId,
                    OrganizationCode = f.OrganizationCode,
                    DateUploaded = f.DateUploaded
                })
                .ToListAsync();
        }

        public async Task<bool> Insert(OrganizationFilesDto file)
        {
            var entitie = _mapper.Map<OrganizationFiles>(file);

            await _context.OrganizationFiles.AddAsync(entitie);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> Delete(Guid id)
        {
            var entity = await _context.OrganizationFiles.FindAsync(id);
            if (entity == null)
            {
                return false;
            }

            _context.OrganizationFiles.Remove(entity);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteFiles(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return false;
            }

            var deletedCount = await _context.OrganizationFiles
                .Where(f => ids.Contains(f.Id))
                .ExecuteDeleteAsync();

            return deletedCount > 0;
        }

        public async Task<bool> DeleteByTaskId(Guid taskId)
        {
            var filesToDelete = await _context.OrganizationFiles
                .Where(f => f.TaskId == taskId)
                .ToListAsync();

            if (filesToDelete.Count == 0)
            {
                return false;
            }

            _context.OrganizationFiles.RemoveRange(filesToDelete);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteAllFileProject(Guid projectId)
        {
            var filesToDelete = await _context.OrganizationFiles
                .Where(f => f.ProjectId == projectId)
                .ToListAsync();

            if (filesToDelete.Count == 0)
            {
                return false;
            }

            _context.OrganizationFiles.RemoveRange(filesToDelete);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteAllFile(Guid organizationCode)
        {
            var filesToDelete = await _context.OrganizationFiles
                .Where(f => f.OrganizationCode == organizationCode)
                .ToListAsync();

            if (filesToDelete.Count == 0)
            {
                return false;
            }

            _context.OrganizationFiles.RemoveRange(filesToDelete);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }
    }
}
