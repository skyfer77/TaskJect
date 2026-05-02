namespace Domain.Database
{
	public interface INotificationRepository
	{
		Task<List<NotificationDto>> Retrieve(string userId);
		Task<bool> Insert(List<NotificationDto> notificationsDto);
		Task<bool> SetIsReviewed(Guid id);
		Task<bool> SetIsReviewedAll(string userId);
		Task<bool> Delete(Guid id);
		Task<bool> DeleteOld();
	}
}
