namespace TaskJect.Web.Services
{
    public class FakeGitHubWebhookService : IGitHubWebhookService
    {
        public Task HandleEvent(string eventType, string payload)
            => Task.CompletedTask;
    }
}
