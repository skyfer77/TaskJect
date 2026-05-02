using Data;
using Data.DomainEvent;
using Domain.Database;
using Domain.DomainEvents;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

public class TariffEndCheckFunction
{
    private readonly IServiceProvider _serviceProvider;
	private readonly DomainEventDispatcher _dispatcher;
	private readonly ILogger<TariffEndCheckFunction> _logger;
    public TariffEndCheckFunction(IServiceProvider serviceProvider, DomainEventDispatcher dispatcher,
		ILogger<TariffEndCheckFunction> logger)
    {
        _logger = logger;
		_dispatcher = dispatcher;
        _serviceProvider = serviceProvider;
    }

    [Function("TariffEndCheckFunction")]
    public async Task Run([TimerTrigger("0 0 0 * * *", RunOnStartup = true)] TimerInfo myTimer)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            _logger.LogInformation($"TariffEndCheckFunction executed at: {DateTime.UtcNow}");

			var tariffPlanHistoryRepository = scope.ServiceProvider.GetRequiredService<ITariffPlanHistoryRepository>();
			var allPaidTariffs = await tariffPlanHistoryRepository.RetrieveAllActive(false);

			var domainEvents = new List<IDomainEvent>();

			var now = DateTime.UtcNow;

			foreach (var tariff in allPaidTariffs)
			{
				var dateTo = tariff.DateTo;
				var daysLeft = (dateTo.Date - now.Date).TotalDays;

				if (daysLeft == 7)
				{
					domainEvents.Add(new SubscriptionExpirationInWeekDomainEvent(tariff.OrganizationCode.ToString()));
				}
				else if (daysLeft == 3)
				{
					domainEvents.Add(new SubscriptionExpirationIn3DaysDomainEvent(tariff.OrganizationCode.ToString()));
				}
				else if (dateTo < now)
				{
					var newBasicTariffPlan = new TariffPlanHistoryDto
					{
						OrganizationCode = tariff.OrganizationCode,
						TariffPlanCode = SD.BasicPlanCode,
						DateFrom = now,
						DateTo = new DateTime(9999, 12, 31, 23, 59, 59)
					};

					await tariffPlanHistoryRepository.Insert(newBasicTariffPlan);

					domainEvents.Add(new SubscriptionExpiredDomainEvent(tariff.OrganizationCode.ToString()));
				}

				await _dispatcher.DispatchAsync(domainEvents);
			}
		}
    }
}
