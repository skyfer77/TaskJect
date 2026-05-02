using AutoMapper;
using Domain.Database;
using Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Data.Database.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        public NotificationRepository(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<NotificationDto>> Retrieve(string userId)
        {
            var notifications = await _dbContext.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.Created)
                .ToListAsync();
            return _mapper.Map<List<NotificationDto>>(notifications);
        }

        public async Task<bool> Insert(List<NotificationDto> notificationsDto)
        {
            if (notificationsDto == null || notificationsDto.Count == 0)
            {
                return false;
            }

            var notifications = _mapper.Map<List<Notification>>(notificationsDto);

            await _dbContext.Notifications.AddRangeAsync(notifications);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> SetIsReviewed(Guid id)
        {
            var notification = new Notification { Id = id, IsReviewed = true };
            _dbContext.Attach(notification);
            _dbContext.Entry(notification).Property(x => x.IsReviewed).IsModified = true;

            var affected = await _dbContext.SaveChangesAsync();
            return affected > 0;
        }

        public async Task<bool> SetIsReviewedAll(string userId)
        {
            var notifications = await _dbContext.Notifications
                .Where(n => n.UserId == userId && !n.IsReviewed)
                .ToListAsync();

            notifications.ForEach(n => n.IsReviewed = true);

            var affected = await _dbContext.SaveChangesAsync();
            return affected > 0;
        }

        public async Task<bool> Delete(Guid id)
        {
            var notification = await _dbContext.Notifications
                .FirstOrDefaultAsync(x => x.Id == id);
            if (notification == null)
            {
                return false;
            }
            try
            {
                _dbContext.Notifications.Remove(notification);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

		public async Task<bool> DeleteOld()
		{
			var oneYearAgo = DateTime.UtcNow.Date.AddYears(-1);

			await _dbContext.Database.ExecuteSqlRawAsync(@"
                DELETE FROM Notification
                WHERE IsReviewed = 1
                  AND Created < @oneYearAgo",
				new SqlParameter("@oneYearAgo", oneYearAgo)
            );

			return true;
		}
	}
}