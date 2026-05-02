namespace TaskJect.Web.Enums
{
    [Flags]
    public enum CookieConsentType
    {
        NecessaryOnly = 1,
        Functional = 2,
        Analytics = 4,
        Performance = 8,
        Advertisement =16,
        All = NecessaryOnly | Functional | Analytics | Performance | Advertisement,
    }
}
