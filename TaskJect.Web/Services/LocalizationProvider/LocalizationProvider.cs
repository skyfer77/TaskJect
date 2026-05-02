using TaskJect.Web.Resources;
using Microsoft.Extensions.Localization;

namespace TaskJect.Web.Services
{
    public class LocalizationProvider : ILocalizationProvider
    {
        private readonly IStringLocalizer _localizer;

        public LocalizationProvider(IStringLocalizer<SharedResources> localizer)
        {
            _localizer = localizer;
        }

        public Dictionary<string, string> GetAllTranslations()
        {
            return _localizer
                .GetAllStrings(includeParentCultures: true)
                .ToDictionary(x => x.Name, x => x.Value);
        }
    }
}
