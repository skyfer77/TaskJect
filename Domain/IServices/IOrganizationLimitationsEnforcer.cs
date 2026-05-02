using Domain.Database;

namespace Domain.IServices
{
    public interface IOrganizationLimitationsEnforcer
    {
        Task<bool> UnlockUsers(string organizationId, string planCode);
        Task<bool> CleanupExceededLimits(List<Guid> organizationsIds);
        Task<bool> ApplyTariffPlan(TariffPlanHistoryDto tariffHistory, bool isRefund);
    }
}
