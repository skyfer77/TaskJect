using Domain.Database;
using Data;

namespace TaskJect.Web.Services
{
    public class TariffPlansInitializer : ITariffPlansInitializer
    {
        private readonly ITariffPlanHistoryRepository _tariffPlanHistoryRepository;
        public TariffPlansInitializer(ITariffPlanHistoryRepository tariffPlanHistoryRepository)
        {
            _tariffPlanHistoryRepository = tariffPlanHistoryRepository;
        }
        public async System.Threading.Tasks.Task InitializeAsync()
        {
            var organizationsWithoutTariff = await _tariffPlanHistoryRepository.RetrieveAllOrganizationsWithoutTariff();
            foreach (var organization in organizationsWithoutTariff)
            {
                var newTariffPlanHistory = new TariffPlanHistoryDto();
                newTariffPlanHistory.OrganizationCode = organization.OrganizationId;
                newTariffPlanHistory.TariffPlanCode = SD.BasicPlanCode;
                newTariffPlanHistory.DateFrom = DateTime.UtcNow.Date;
                newTariffPlanHistory.DateTo = new DateTime(9999, 12, 31, 23, 59, 59);
                var tariffHistoryResponse = await _tariffPlanHistoryRepository.Insert(newTariffPlanHistory);
            }
        }
    }
}
