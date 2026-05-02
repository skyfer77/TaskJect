using Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Domain.Database;

namespace Data.Database.Repository
{
    internal class GumroadWebhookLogRepository : IGumroadWebhookLogRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public GumroadWebhookLogRepository(ApplicationDbContext db) => _dbContext = db;

        public Task<bool> ExistsAsync(string eventId, CancellationToken ct = default)
            => _dbContext.Set<GumroadWebhookLog>().AnyAsync(x => x.EventId == eventId, ct);

        public async System.Threading.Tasks.Task AddAsync(GumroadWebhookLog log, CancellationToken ct = default)
        {
            _dbContext.Set<GumroadWebhookLog>().Add(log);
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
