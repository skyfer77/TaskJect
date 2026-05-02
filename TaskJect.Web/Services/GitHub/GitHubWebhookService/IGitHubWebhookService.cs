namespace TaskJect.Web.Services
{
    public interface IGitHubWebhookService
    {
        Task HandleEvent(string eventType, string payload);
    }
}
