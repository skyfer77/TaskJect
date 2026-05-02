using AutoMapper;
using Domain.Database;
using Data.DbContexts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Data.Database.Repository
{
    public class TariffPlanHistoryRepository : ITariffPlanHistoryRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IMapper _mapper;
        public TariffPlanHistoryRepository(ApplicationDbContext applicationDbContext, IMapper mapper)
        {
            _applicationDbContext = applicationDbContext;
            _mapper = mapper;
        }

        public async Task<bool> Insert(TariffPlanHistoryDto newTariffPlanHistory)
        {
            var tariffPlanHistory = _mapper.Map<TariffPlanHistory>(newTariffPlanHistory);
            try
            {
                _applicationDbContext.TariffPlansHistories.Add(tariffPlanHistory);

                await _applicationDbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<IEnumerable<TariffPlanHistoryDto>> RetrieveByTariff(string tariffPlanCode, bool onlyActive)
        {
            var tariffPlanHistory = await _applicationDbContext.TariffPlansHistories.Where(x => x.TariffPlanCode == tariffPlanCode && (!onlyActive || x.DateTo > DateTime.UtcNow)).ToListAsync();
            return _mapper.Map<List<TariffPlanHistoryDto>>(tariffPlanHistory);
        }
        public async Task<IEnumerable<TariffPlanHistoryDto>> RetrieveByOrganization(Guid organizationId)
        {
            var tariffPlanHistory = await _applicationDbContext.TariffPlansHistories.Where(x => x.OrganizationCode == organizationId).ToListAsync();
            return _mapper.Map<List<TariffPlanHistoryDto>>(tariffPlanHistory);
        }
        public async Task<TariffPlanHistoryDto> RetrieveActive(Guid organizationId)
        {
            var now = DateTime.UtcNow;

            var tariffPlanHistory = await _applicationDbContext.TariffPlansHistories.Where(x => x.OrganizationCode == organizationId 
            && x.DateFrom <= now && x.DateTo > now).OrderByDescending(x => x.DateFrom).FirstOrDefaultAsync();

            if (tariffPlanHistory != null)
            {
                return _mapper.Map<TariffPlanHistoryDto>(tariffPlanHistory);
            }
            return null;
        }
        public async Task<TariffPlanHistoryDto> RetrieveLatest(Guid organizationId)
        {
            var now = DateTime.UtcNow;

            var tariffPlanHistory = await _applicationDbContext.TariffPlansHistories.Where(x => x.OrganizationCode == organizationId).OrderByDescending(x => x.DateFrom).FirstOrDefaultAsync();
            if (tariffPlanHistory != null)
            {
                return _mapper.Map<TariffPlanHistoryDto>(tariffPlanHistory);
            }
            return null;
        }

        public async Task<Dictionary<Guid, TariffPlanHistoryDto>> RetrieveActiveByIds(List<Guid> organizationIds)
        {
            if (organizationIds == null || organizationIds.Count == 0)
            {
                return new Dictionary<Guid, TariffPlanHistoryDto>();
            }
                
            var now = DateTime.UtcNow;

            var tariffPlanHistories = await _applicationDbContext.TariffPlansHistories
                .Where(x =>
                    organizationIds.Contains(x.OrganizationCode) &&
                    x.DateFrom <= now &&
                    x.DateTo > now
                )
                .GroupBy(x => x.OrganizationCode)
                .Select(g => g
                    .OrderByDescending(x => x.DateFrom)
                    .FirstOrDefault()
                )
                .ToListAsync();

            return tariffPlanHistories
                .Where(x => x != null)
                .ToDictionary(
                    x => x.OrganizationCode,
                    x => _mapper.Map<TariffPlanHistoryDto>(x)
                );
        }

        public async Task<TariffPlanHistoryDto> Retrieve(string subscribtionCode)
        {
            var tariffPlanHistory = await _applicationDbContext.TariffPlansHistories.Where(x => x.SubscriptionCode == subscribtionCode).OrderByDescending(x => x.DateFrom).FirstOrDefaultAsync();
            if (tariffPlanHistory != null)
            {
                return _mapper.Map<TariffPlanHistoryDto>(tariffPlanHistory);
            }
            return null;
        }
        public async Task<List<OrganizationDto>> RetrieveAllOrganizationsWithoutTariff()
        {
            var result = from org in _applicationDbContext.Organizations
                         join tariff in _applicationDbContext.TariffPlansHistories
                         on org.OrganizationId equals tariff.OrganizationCode into joined
                         from subTariff in joined.DefaultIfEmpty()
                         where subTariff == null
                         select org;

            var organizationsWithoutTariff = await result.ToListAsync();
            return _mapper.Map<List<OrganizationDto>>(organizationsWithoutTariff);

        }
        public async Task<bool> Delete(Guid organizationCode, string tariffPlanCode, DateTime dateFrom)
        {
            var tariffPlanHistory = await _applicationDbContext.TariffPlansHistories.FirstOrDefaultAsync(x => x.OrganizationCode == organizationCode
                              && x.TariffPlanCode == tariffPlanCode
                              && x.DateFrom == dateFrom);
            if (tariffPlanHistory == null)
            {
                return false;
            }
            _applicationDbContext.TariffPlansHistories.Remove(tariffPlanHistory);
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteAll(Guid organizationCode)
        {
            try
            {
                string sql = "DELETE FROM TariffPlanHistory WHERE OrganizationCode = @organizationCode";

                var parameter = new SqlParameter("@organizationCode", organizationCode);

                await _applicationDbContext.Database.ExecuteSqlRawAsync(sql, parameter);

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> Update(TariffPlanHistoryDto tariffPlanHistoryDto)
        {
            var tariffPlanHistory = await _applicationDbContext.TariffPlansHistories
                .FirstOrDefaultAsync(x => (x.OrganizationCode == tariffPlanHistoryDto.OrganizationCode) && (x.TariffPlanCode == tariffPlanHistoryDto.TariffPlanCode) && (x.DateFrom == tariffPlanHistoryDto.DateFrom));

            if (tariffPlanHistory != null)
            {
                _mapper.Map(tariffPlanHistoryDto, tariffPlanHistory);

                _applicationDbContext.TariffPlansHistories.Update(tariffPlanHistory);

                await _applicationDbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }
        public async Task<List<TariffPlanHistoryDto>> RetrieveAllActive(bool includeBasicTariff)
        {
            var query = _applicationDbContext.TariffPlansHistories
                .Where(x => (includeBasicTariff || x.TariffPlanCode != SD.BasicPlanCode) && x.DateFrom <= DateTime.UtcNow && x.DateTo > DateTime.UtcNow)
                .GroupBy(x => x.OrganizationCode)
                .Select(g => g.OrderByDescending(x => x.DateFrom).FirstOrDefault());

            var result = await query.ToListAsync();

            return _mapper.Map<List<TariffPlanHistoryDto>>(result);
        }
        public async Task<Dictionary<Guid, List<TariffPlanHistoryDto>>> RetrieveForAllLastFour()
        {
            var now = DateTime.UtcNow;

            var data = await _applicationDbContext.TariffPlansHistories
                .Where(x =>
                    x.DateFrom <= now)
                .GroupBy(x => x.OrganizationCode)
                .Select(g => new
                {
                    OrganizationId = g.Key,
                    Tariffs = g
                        .OrderByDescending(x => x.DateFrom)
                        .Take(4)
                        .ToList()
                })
                .ToListAsync();

            return data.ToDictionary(
                x => x.OrganizationId,
                x => _mapper.Map<List<TariffPlanHistoryDto>>(x.Tariffs)
            );
        }
    }
}
