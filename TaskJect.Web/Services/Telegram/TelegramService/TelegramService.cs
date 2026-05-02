using TaskJect.Web.Common;
using Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace TaskJect.Web.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly HttpClient _httpClient;
        private readonly string _botToken;
        private readonly string _apiUrl;
        private readonly string _webhookUrl;
        private readonly ApplicationDbContext _dbContext;

        public TelegramService(IOptions<TelegramOptions> options, ApplicationDbContext dbContext)
        {
            var settings = options.Value;
            _botToken = settings.BotToken;
            _apiUrl = $"https://api.telegram.org/bot{_botToken}";
            _webhookUrl = settings.WebhookUrl;
            _dbContext = dbContext;
            _httpClient = new HttpClient();
        }

        public async System.Threading.Tasks.Task RegisterWebhookAsync()
        {
            var url = $"{_apiUrl}/setWebhook?url={_webhookUrl}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Could not install webhook: {error}");
            }
        }

        public async Task<TelegramRegistrationResult> RegisterUserByTicketAsync(string chatId, string ticket, string? telegramUsername)
        {
            if (string.IsNullOrWhiteSpace(ticket))
            {
                return new TelegramRegistrationResult(false) { ErrorMessage = "A ticket cannot be empty" };
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.TelegramTicket == ticket);
            if (user == null)
            {
                return new TelegramRegistrationResult(false) { ErrorMessage = "User with this ticket was not found" };
            }
            if (user.TelegramChatId != null)
            {
                return new TelegramRegistrationResult(false) { ErrorMessage = "The user is already registered in Telegram" };
            }

            user.TelegramChatId = chatId;
            user.TelegramUserName = telegramUsername;
            await _dbContext.SaveChangesAsync();

            return new TelegramRegistrationResult(true);
        }

        public async System.Threading.Tasks.Task SendMessageAsync(string chatId, string message)
        {
            var longChatId = long.Parse(chatId);
            var url = $"{_apiUrl}/sendMessage?chat_id={longChatId}&text={Uri.EscapeDataString(message)}";
            await _httpClient.GetAsync(url);
        }

        public async Task<TelegramSendMessageResult> SendMessageByUserIdAsync(string userId, string message)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return TelegramSendMessageResult.Fail("User not found");
            }

            if (string.IsNullOrEmpty(user.TelegramChatId))
            {
                return TelegramSendMessageResult.Fail("The user has not linked a Telegram account");
            }

            await SendMessageAsync(user.TelegramChatId, message);
            return TelegramSendMessageResult.Ok();
        }

        public async System.Threading.Tasks.Task SendMessageToAllAsync(string message)
        {
            var users = await _dbContext.Users
                .Where(u => u.TelegramChatId != null)
                .ToListAsync();
            foreach (var user in users)
            {
                await SendMessageAsync(user.TelegramChatId, message);
            }
        }
    }

    public class TelegramRegistrationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public TelegramRegistrationResult(bool success)
        {
            Success = success;
        }
    }

    public class TelegramSendMessageResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }

        public static TelegramSendMessageResult Ok()
        {
            return new TelegramSendMessageResult { Success = true };
        }

        public static TelegramSendMessageResult Fail(string errorMessage)
        {
            return new TelegramSendMessageResult { Success = false, ErrorMessage = errorMessage };
        }
    }

}
