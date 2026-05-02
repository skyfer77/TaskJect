using Data;
namespace TaskJect.Web.Services
{
    public interface IGumroadLinkProvider
    {
        public string GetGumroadLink(SD.Gumroad.ProductType productType, Guid organizationId);
    }
}
