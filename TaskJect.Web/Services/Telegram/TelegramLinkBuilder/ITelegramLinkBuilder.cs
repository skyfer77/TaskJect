namespace TaskJect.Web.Services
{
    public interface ITelegramLinkBuilder
    {
        string BuildLink(string telegramTicket);
    }
}
