namespace TaskJect.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DisplayNotificationAttribute : Attribute
    {
        public string TitleKey { get; }
        public string MessageKey { get; }

        public DisplayNotificationAttribute(string titleKey, string messageKey)
        {
            TitleKey = titleKey;
            MessageKey = messageKey;
        }
    }
}