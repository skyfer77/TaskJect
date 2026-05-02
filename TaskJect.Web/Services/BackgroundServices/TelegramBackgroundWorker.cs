namespace TaskJect.Web.Services.BackgroundServices
{
    public class TelegramBackgroundWorker : BackgroundService
    {
        private readonly TelegramMessageQueue _messageQueue;
        private readonly IServiceScopeFactory _scopeFactory;
        public TelegramBackgroundWorker(TelegramMessageQueue messageQueue,
            IServiceScopeFactory scopeFactory)
        {
            _messageQueue = messageQueue;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_messageQueue.TryDequeue(out var message))
                {
                    try
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramService>();

                            await telegramService.SendMessageByUserIdAsync(message.userId.ToString(), message.message);
                        }
                    }
                    catch (Exception ex)
                    {
                        // логувати помилку
                    }

                    await Task.Delay(100, stoppingToken); 
                }
                else
                {
                    await Task.Delay(500, stoppingToken);
                }
            }
        }
    }

}
