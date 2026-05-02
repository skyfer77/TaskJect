namespace TaskJect.Web.Models
{
    public class TelegramUpdate
    {
        public long UpdateId { get; set; }
        public TelegramMessage? Message { get; set; }
    }

    public class TelegramMessage
    {
        public long MessageId { get; set; }
        public TelegramChat Chat { get; set; }
        public string? Text { get; set; }
    }

    public class TelegramChat
    {
        public long Id { get; set; }
        public string? FirstName { get; set; }
        public string? Username { get; set; }
        public string? Type { get; set; }
    }
}
