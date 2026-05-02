using TaskJect.Web.Attributes;
using TaskJect.Web.Enums;

namespace TaskJect.Web.Services
{
	public static class NotificationTypeExtensions
	{
		public static DisplayNotificationAttribute GetDisplayAttribute(this NotificationType type)
		{
			var memberInfo = type.GetType().GetMember(type.ToString());
			return memberInfo[0]
				.GetCustomAttributes(typeof(DisplayNotificationAttribute), false)
				.FirstOrDefault() as DisplayNotificationAttribute;
		}
	}
}
