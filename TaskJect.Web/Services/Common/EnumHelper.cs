using TaskJect.Web.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace TaskJect.Web.Services
{
    public static class EnumHelper
    {
        public static string GetEnumDisplayName(this Enum value)
        {
            var displayName = value.GetType()
                .GetMember(value.ToString())
                .FirstOrDefault()?
                .GetCustomAttribute<DisplayAttribute>()?
                .GetName();

            return displayName ?? value.ToString();
        }

        public static Dictionary<string, string> GetEnumDisplayNameDictionary(Dictionary<string,int> properties)
        {
            return properties.ToDictionary(
                prop => GetEnumDisplayName(Enum.Parse<UserAction>(prop.Key)),
                prop => GetEnumDisplayName(Enum.Parse<ProjectPermission>(prop.Value.ToString()))
            );
        }
    }
}
