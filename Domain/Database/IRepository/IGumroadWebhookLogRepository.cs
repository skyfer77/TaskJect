using Domain.Database;

namespace Domain.Database
{
    public interface IGumroadWebhookLogRepository
    {
        Task<bool> ExistsAsync(string eventId, CancellationToken ct = default);
        System.Threading.Tasks.Task AddAsync(GumroadWebhookLog log, CancellationToken ct = default);
    }
}
