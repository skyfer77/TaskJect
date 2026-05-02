using Data;
using Domain.Database;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class LockingNewestMembersFunction
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LockingNewestMembersFunction> _logger;
    public LockingNewestMembersFunction(IServiceProvider serviceProvider,
        ILogger<LockingNewestMembersFunction> logger)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    [Function("LockingNewestMembersFunction")]
    public async System.Threading.Tasks.Task Run([TimerTrigger("0 0 */4 * * *", RunOnStartup = true)] TimerInfo myTimer)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            _logger.LogInformation($"LockingNewestMembersFunction executed at: {DateTime.UtcNow}");

            var applicationUserRepository = scope.ServiceProvider.GetRequiredService<IApplicationUserRepository>();
            var tariffPlanHistoryRepository = scope.ServiceProvider.GetRequiredService<ITariffPlanHistoryRepository>();
            var tariffPlanRepository = scope.ServiceProvider.GetRequiredService<ITariffPlanRepository>();

            var allTariffs = await tariffPlanRepository.Retrieve();
            var allActiveTariffs = await tariffPlanHistoryRepository.RetrieveAllActive(true);

            var maxUsersByOrganization = allActiveTariffs
                .Where(h => allTariffs.ContainsKey(h.TariffPlanCode))
                .ToDictionary(
                    h => h.OrganizationCode,
                    h => allTariffs[h.TariffPlanCode].MaxUsers
                );

            var organizationUserInfo = await applicationUserRepository.GetOrganizationUserInfo();

            var exceededMaxUsersByOrganization = organizationUserInfo
                    .Where(o => maxUsersByOrganization.TryGetValue( Guid.Parse(o.OrganizationId), out var maxUsers) && o.CountUserOrganization > maxUsers)
                    .ToDictionary( o => o.OrganizationId, o => maxUsersByOrganization[Guid.Parse(o.OrganizationId)]);

            var allExceededUsersIds = (await applicationUserRepository.GetExceededUsersByOrganizations(exceededMaxUsersByOrganization)).SelectMany(kvp => kvp.Value).ToList();
            var result = await applicationUserRepository.LockoutUsersByIds(allExceededUsersIds, DateTime.MaxValue);
            _logger.LogInformation($"LockingNewestMembersFunction result is: {result}");

        }
    }
}
