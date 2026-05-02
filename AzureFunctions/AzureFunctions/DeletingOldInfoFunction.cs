using Data;
using Domain.Database;
using Domain.IServices;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class DeletingOldInfoFunction
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LockingNewestMembersFunction> _logger;
    public DeletingOldInfoFunction(IServiceProvider serviceProvider,
        ILogger<LockingNewestMembersFunction> logger)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    [Function("DeletingOldInfoFunction")]
    public async System.Threading.Tasks.Task Run([TimerTrigger("0 0 0 * * *", RunOnStartup = true)] TimerInfo myTimer)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            _logger.LogInformation($"DeletingOldInfoFunction executed at: {DateTime.UtcNow}");

            var applicationUserRepository = scope.ServiceProvider.GetRequiredService<IApplicationUserRepository>();
            var tariffPlanHistoryRepository = scope.ServiceProvider.GetRequiredService<ITariffPlanHistoryRepository>();
            var tariffPlanRepository = scope.ServiceProvider.GetRequiredService<ITariffPlanRepository>();

            var organizationLimitationsEnforcer = scope.ServiceProvider.GetRequiredService<IOrganizationLimitationsEnforcer>();

            var allTariffs = await tariffPlanRepository.Retrieve();
            var lastFourTariffsDict = await tariffPlanHistoryRepository.RetrieveForAllLastFour();

            var organizationIds = new List<Guid>();
            var now = DateTime.UtcNow;

            foreach (var kvp in lastFourTariffsDict)
            {
                var tariffs = kvp.Value;

                if (tariffs == null || tariffs.Count == 0)
                {
                    continue;
                }

                var lastTariff = tariffs[0]; 

                if (lastTariff.TariffPlanCode == SD.BasicPlanCode &&lastTariff.DateFrom.AddMonths(3) <= now)
                {
                    organizationIds.Add(kvp.Key);
                    continue;
                }

                if (tariffs.Count < 4)
                {
                    continue;
                }

                var planCode = lastTariff.TariffPlanCode;
                bool allSame = true;

                for (int i = 1; i < tariffs.Count; i++)
                {
                    if (tariffs[i].TariffPlanCode != planCode)
                    {
                        allSame = false;
                        break;
                    }
                }

                if (allSame && tariffs[3].DateFrom.AddMonths(3) <= now)
                {
                    organizationIds.Add(kvp.Key);
                }
            }

            var result = await organizationLimitationsEnforcer.CleanupExceededLimits(organizationIds);
            _logger.LogInformation($"DeletingOldInfoFunction result is: {result}");

        }
    }
}
