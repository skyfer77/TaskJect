namespace Domain.Database
{
    public interface ITariffPlanRepository
    {
        Task<bool> Insert(TariffPlanDto newTariffPlan);
        Task<Dictionary<string, TariffPlanDto>> Retrieve();
        Task<List<TariffPlanDto>> RetrievePlansList(string currentTariff, string? source = null);
        Task<List<TariffPlanDto>> RetrievePublicPlansList(string? source = null);
        Task<TariffPlanDto> Retrieve(string tariffPlanCode);
        Task<bool> Delete(string tariffPlanCode);

    }
}
