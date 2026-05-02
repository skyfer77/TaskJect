using TaskJect.Web.Models;

namespace TaskJect.Web.Services
{
    public interface INotificationService
	{
		Task SendNotification(SystemEvent systemEvent);
	}
}
