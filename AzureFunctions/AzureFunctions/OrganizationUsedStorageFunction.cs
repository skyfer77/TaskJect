using Domain.Database;
using Domain.IServices;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public class OrganizationUsedStorageFunction
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IDataSizeCalculator _dataSizeCalculator;
    private readonly ILogger<OrganizationUsedStorageFunction> _logger;
    public OrganizationUsedStorageFunction(IOrganizationRepository organizationRepository,
        IDataSizeCalculator dataSizeCalculator,
        ILogger<OrganizationUsedStorageFunction> logger)
    {
        _logger = logger;
        _organizationRepository = organizationRepository;
        _dataSizeCalculator = dataSizeCalculator;
    }
    [Function("OrganizationUsedStorageFunction")]
    public async System.Threading.Tasks.Task Run([TimerTrigger("0 0 */4 * * *", RunOnStartup = true)] TimerInfo timer)
    {
        _logger.LogInformation($"OrganizationUsedStorageFunction executed at: {DateTime.UtcNow}");
        var organizations = await _organizationRepository.Retrieve();

        foreach (var organization in organizations)
        {
            var usedStorage = await _dataSizeCalculator.CalculateOrganizationDataSize(organization.OrganizationId.ToString());
            organization.UsedStorageSpace = usedStorage;
            await _organizationRepository.Update(organization);
        }
    }
}
