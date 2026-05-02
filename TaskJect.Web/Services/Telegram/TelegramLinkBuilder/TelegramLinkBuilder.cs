using TaskJect.Web.Common;
using Microsoft.Extensions.Options;

namespace TaskJect.Web.Services
{
    internal class TelegramLinkBuilder : ITelegramLinkBuilder
    {
        private readonly TelegramOptions _telegramOptions;

        public TelegramLinkBuilder(IOptions<TelegramOptions> options)
        {
            _telegramOptions = options.Value;
        }

        public string BuildLink(string telegramTicket)
        {
            return $"https://t.me/{_telegramOptions.BotName}?start={telegramTicket}";
        }
    }
}
