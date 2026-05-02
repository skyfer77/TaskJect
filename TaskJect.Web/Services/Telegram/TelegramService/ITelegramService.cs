namespace TaskJect.Web.Services
{
    public interface ITelegramService
    {
        public Task RegisterWebhookAsync();
        public Task<TelegramRegistrationResult> RegisterUserByTicketAsync(string chatId, string ticket, string? telegramUsername);
        public Task SendMessageAsync(string chatId, string message);
        public Task<TelegramSendMessageResult> SendMessageByUserIdAsync(string userId, string message);
        public Task SendMessageToAllAsync(string message);
    }
}
