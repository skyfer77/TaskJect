using AutoMapper;
using Data.DbContexts;
using Domain.Enums;
using Domain.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Data.Database.Repository
{
    internal class OrganizationAppealRepository : IOrganizationAppealRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private IMapper _mapper;
        public OrganizationAppealRepository(ApplicationDbContext DbContext, IMapper mapper)
        {
            _dbContext = DbContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrganizationAppealDto>> Retrieve()
        {
            var appeals = await _dbContext.OrganizationAppeals.ToListAsync();
            return _mapper.Map<List<OrganizationAppealDto>>(appeals);
        }

        public async Task<OrganizationAppealDto> Retrieve(Guid Id)
        {
            var appeal = await _dbContext.OrganizationAppeals
                .FirstOrDefaultAsync(x => x.Id == Id);
            return _mapper.Map<OrganizationAppealDto>(appeal);
        }

        public async Task<OrganizationAppealDto> RetrieveByOrganization(Guid organizationId)
        {
            var appeal = await _dbContext.OrganizationAppeals
                .FirstOrDefaultAsync(x => x.OrganizationCode == organizationId);
            return _mapper.Map<OrganizationAppealDto>(appeal);
        }

        public async Task<int> RetrieveCountThisMonth(Guid organizationId)
        {
            var currentDate = DateTime.Now;
            var startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddSeconds(-1);

            var totalAppeals = await _dbContext.OrganizationAppeals
                .Where(x => x.OrganizationCode == organizationId && x.Date >= startOfMonth && x.Date <= endOfMonth)
                .CountAsync();

            return totalAppeals;
        }

        public async Task<bool> Insert(OrganizationAppealDto appealDto)
        {
            var appeal = _mapper.Map<OrganizationAppeal>(appealDto);
            appeal.Date = DateTime.Now;
            appeal.Status = AppealStatus.InProcessing;
            try
            {
                _dbContext.OrganizationAppeals.Add(appeal);

                appeal.MarkAsSended();
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Update(OrganizationAppealDto appealDto)
        {
            var appeal = await _dbContext.OrganizationAppeals
                .FirstOrDefaultAsync(o => o.Id == appealDto.Id);

            if (appeal != null)
            {
                _mapper.Map(appealDto, appeal);

                _dbContext.OrganizationAppeals.Update(appeal);

                await _dbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }

        [Authorize(Roles = "Moderator, Admin, God")]
        public async Task<bool> Delete(Guid Id)
        {
            var appeal = await _dbContext.OrganizationAppeals
                .FirstOrDefaultAsync(x => x.Id == Id);

            if (appeal == null)
            {
                return false;
            }
            _dbContext.OrganizationAppeals.Remove(appeal);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
