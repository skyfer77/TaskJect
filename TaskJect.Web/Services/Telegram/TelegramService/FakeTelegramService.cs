
namespace TaskJect.Web.Services
{
    public class FakeTelegramService : ITelegramService
    {
        public async Task<TelegramRegistrationResult> RegisterUserByTicketAsync(string chatId, string ticket, string? telegramUsername)
        {
            return new TelegramRegistrationResult(false);
        }

        public Task RegisterWebhookAsync()
        {
            return Task.CompletedTask;
        }

        public Task SendMessageAsync(string chatId, string message)
        {
            return Task.CompletedTask;
        }

        public async Task<TelegramSendMessageResult> SendMessageByUserIdAsync(string userId, string message)
        {
            return TelegramSendMessageResult.Fail("Its fake telegram service");
        }

        public Task SendMessageToAllAsync(string message)
        {
            return Task.CompletedTask;
        }
    }
}
