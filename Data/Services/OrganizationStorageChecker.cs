using Domain.IServices;
using Domain.Database;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Services
{
    internal class OrganizationStorageChecker : IOrganizationStorageChecker
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public OrganizationStorageChecker(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        //Key - Organization code, value - IsAvailableStotage
        private Dictionary<Guid, bool> _cache;
        public async Task<bool> CheckAsync(Guid organizationCode)
        {
            //TODO: десь не чиститься кеш, через що нові організації не потрапляють в кеш і сервіс повертає false
            //if (_cache == null)
            //{
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    _cache = new Dictionary<Guid, bool>();
                    var organizationRepository = scope.ServiceProvider.GetRequiredService<IOrganizationRepository>();
                    var tariffPlanHistoryRepository = scope.ServiceProvider.GetRequiredService<ITariffPlanHistoryRepository>();
                    var organizations = await organizationRepository.Retrieve();
                    var tariffPlanRepository = scope.ServiceProvider.GetRequiredService<ITariffPlanRepository>();
                    var tariffs = await tariffPlanRepository.Retrieve();
                    foreach (var organization in organizations)
                    {
                        var currentTariffHistory = await tariffPlanHistoryRepository.RetrieveActive(organization.OrganizationId);
                        if(currentTariffHistory != null) 
                        {
                            if (tariffs.TryGetValue(currentTariffHistory.TariffPlanCode, out var tariff))
                            {
                                _cache[organization.OrganizationId] = organization.UsedStorageSpace <= tariff.MaxStorageBytes;
                            }
                        }
                        else
                        {
                            if (tariffs.TryGetValue(SD.BasicPlanCode, out var tariff)) 
                            {
                                _cache[organization.OrganizationId] = organization.UsedStorageSpace <= tariff.MaxStorageBytes;
                            }
                        }
                       
                    }
                }
            //}
            if(_cache.TryGetValue(organizationCode, out bool isAvailable))
            {
                return isAvailable;
            }
            return false;
        }

        public void ClearCache()
        {
            _cache = null;
        }
    }
}
