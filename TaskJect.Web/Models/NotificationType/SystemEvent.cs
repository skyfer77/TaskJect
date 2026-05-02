using TaskJect.Web.Enums;

namespace TaskJect.Web.Models
{
    public class SystemEvent
    {
        public List<string>? ParticipantIds { get; set; }
        public NotificationType EventType { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }

        public SystemEvent(NotificationType notificationType)
        {
            EventType = notificationType;
        }

        public SystemEvent(List<string> participantIds, NotificationType notificationType)
        {
            ParticipantIds = participantIds;
            EventType = notificationType;
        }

        public SystemEvent(string userId, NotificationType notificationType)
        {
            ParticipantIds = new List<string>() { userId };
            EventType = notificationType;
        }

        public SystemEvent(string userId, NotificationType notificationType, string message)
        {
            ParticipantIds = new List<string>() { userId };
            EventType = notificationType;
            Message = message;
        }

        public SystemEvent(string userId, string title, string message)
        {
            ParticipantIds = new List<string>() { userId };
            Title = title;
            Message = message;
        }
    }
}