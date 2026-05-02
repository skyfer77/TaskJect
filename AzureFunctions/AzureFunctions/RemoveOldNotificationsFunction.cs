using Domain.Database;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class RemoveOldNotificationsFunction
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<RemoveOldNotificationsFunction> _logger;
	public RemoveOldNotificationsFunction(IServiceProvider serviceProvider,
		ILogger<RemoveOldNotificationsFunction> logger)
	{
		_logger = logger;
		_serviceProvider = serviceProvider;
	}

	[Function("RemoveOldNotificationsFunction")]
	public async System.Threading.Tasks.Task Run([TimerTrigger("0 0 0 * * *", RunOnStartup = true)] TimerInfo timer)
	{
		using (var scope = _serviceProvider.CreateScope())
		{
			_logger.LogInformation($"RemoveOldNotificationsFunction executed at: {DateTime.UtcNow}");
			var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
			await notificationRepository.DeleteOld();
		}
	}
}
