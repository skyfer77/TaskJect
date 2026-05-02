using Data;
namespace TaskJect.Web.Services
{
    public class GumroadLinkProvider : IGumroadLinkProvider
    {
        private const string _gumroadBaseUrl = "https://taskject.gumroad.com/l";

        private readonly AesEncryptionHelper _encryptor;
        public GumroadLinkProvider(AesEncryptionHelper aesEncryption)
        {
            _encryptor = aesEncryption;
        }
        public string GetGumroadLink(SD.Gumroad.ProductType productType, Guid organizationId)
        {
            var encrypted = _encryptor.Encrypt(organizationId);
            var variantValue = string.Empty;
            switch(productType)
            {
                case SD.Gumroad.ProductType.StarterPlan:
                    variantValue = SD.Gumroad.StarterVariant;
                    break;
                case SD.Gumroad.ProductType.ProPlan:
                    variantValue = SD.Gumroad.ProVariant;
                    break;
                case SD.Gumroad.ProductType.BusinessPlan:
                    variantValue = SD.Gumroad.BusinessVariant;
                    break;
                case SD.Gumroad.ProductType.EnterprisePlan:
                    variantValue = SD.Gumroad.EnterpriseVariant;
                    break;
            }

            return $"{_gumroadBaseUrl}/{SD.Gumroad.ProductCode}?variant={variantValue}&wanted=1&custom={encrypted}";
        }
    }
}
