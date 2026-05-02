namespace Domain.Database
{
    public interface ITariffPlanHistoryRepository
    {
        Task<bool> Insert(TariffPlanHistoryDto newTariffPlanHistory);
        Task<TariffPlanHistoryDto> RetrieveActive(Guid organizationId);
        Task<TariffPlanHistoryDto> RetrieveLatest(Guid organizationId);
        Task<TariffPlanHistoryDto> Retrieve(string subscribtionCode);
        Task<IEnumerable<TariffPlanHistoryDto>> RetrieveByTariff(string tariffPlanCode, bool onlyActive);
        Task<IEnumerable<TariffPlanHistoryDto>> RetrieveByOrganization(Guid organizationId);
        Task<List<OrganizationDto>> RetrieveAllOrganizationsWithoutTariff();
        Task<bool> Delete(Guid organizationCode, string tariffPlanCode, DateTime dateFrom);
        Task<bool> DeleteAll(Guid organizationCode);
        Task<bool> Update(TariffPlanHistoryDto tariffPlanHistoryDto);
        Task<List<TariffPlanHistoryDto>> RetrieveAllActive(bool includeBasicTariff);
        Task<Dictionary<Guid, TariffPlanHistoryDto>> RetrieveActiveByIds(List<Guid> organizationIds);
        Task<Dictionary<Guid, List<TariffPlanHistoryDto>>> RetrieveForAllLastFour();
    }
}
