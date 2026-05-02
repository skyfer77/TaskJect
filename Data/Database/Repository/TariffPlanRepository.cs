using AutoMapper;
using Domain.Database;
using Data.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Data.Database.Repository
{
    public class TariffPlanRepository : ITariffPlanRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private IMapper _mapper;
        public TariffPlanRepository(ApplicationDbContext applicationDbContext, IMapper mapper)
        {
            _applicationDbContext = applicationDbContext;
            _mapper = mapper;
        }

        [Authorize(Roles = "Moderator, Admin, God")]
        public async Task<bool> Insert(TariffPlanDto newTariffPlan)
        {
            var tariffPlan = _mapper.Map<TariffPlan>(newTariffPlan);
            try
            {
                _applicationDbContext.TariffPlans.Add(tariffPlan);

                await _applicationDbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Dictionary<string, TariffPlanDto>> Retrieve()
        {
            var tariffPlans = await _applicationDbContext.TariffPlans.ToListAsync();
            return _mapper.Map<List<TariffPlanDto>>(tariffPlans).ToDictionary(k => k.Code, v => v);
        }

        public async Task<List<TariffPlanDto>> RetrievePlansList(string currentTariff, string? source = null)
        {
            var tariffPlans = await _applicationDbContext.TariffPlans
                .Where(tp => 
                    (tp.IsPublic 
                    && ((source == null && tp.Source == null) || tp.Source == source))
		            || tp.Code == currentTariff)
                .ToListAsync();
            tariffPlans = tariffPlans.DistinctBy(tp => tp.Code).OrderBy(tp => decimal.Parse(tp.PriceMonth.Replace("$", ""), CultureInfo.InvariantCulture)).ToList();

            return _mapper.Map<List<TariffPlanDto>>(tariffPlans);
        }

        public async Task<List<TariffPlanDto>> RetrievePublicPlansList(string? source = null)
        {
            var tariffPlans = await _applicationDbContext.TariffPlans
                .Where(tp => tp.IsPublic && ((source == null && tp.Source == null) || tp.Source == source)).ToListAsync();
            tariffPlans = tariffPlans.DistinctBy(tp => tp.Code).OrderBy(tp => decimal.Parse(tp.PriceMonth.Replace("$", ""), CultureInfo.InvariantCulture)).ToList();

            return _mapper.Map<List<TariffPlanDto>>(tariffPlans);
        }

		public async Task<TariffPlanDto> Retrieve(string tariffPlanCode)
        {
            var tariffPlan = await _applicationDbContext.TariffPlans.FirstOrDefaultAsync(x => x.Code == tariffPlanCode);
            return _mapper.Map<TariffPlanDto>(tariffPlan);
        }

        [Authorize(Roles = "Moderator, Admin, God")]
        public async Task<bool> Delete(string tariffPlanCode)
        {
            var tariffPlan = await _applicationDbContext.TariffPlans.FirstOrDefaultAsync(x => x.Code == tariffPlanCode);
            if (tariffPlan == null)
            {
                return false;
            }
            _applicationDbContext.TariffPlans.Remove(tariffPlan);
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }
    }
}
