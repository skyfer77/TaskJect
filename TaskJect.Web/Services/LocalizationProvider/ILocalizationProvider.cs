namespace TaskJect.Web.Services
{
    public interface ILocalizationProvider
    {
        Dictionary<string, string> GetAllTranslations();
    }
}