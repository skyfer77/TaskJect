using Domain.Database;
using TaskJect.Web.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Globalization;
using System.Security.Claims;

namespace TaskJect.Web.Controllers
{
    public class CultureController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public CultureController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        public async Task<IActionResult> SetLanguage(string culture, string returnUrl)
        {
            var hasConsent = Request.Cookies.TryGetValue("cookieConsent", out var consentValue)
                                && int.TryParse(consentValue, out var consentInt)
                                && ((CookieConsentType)consentInt & ~CookieConsentType.NecessaryOnly) != 0;

            if (hasConsent)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                
                    user.Culture = culture;
                    await _userManager.UpdateAsync(user);

                    var claims = await _userManager.GetClaimsAsync(user);
                    var existingCultureClaim = claims.FirstOrDefault(c => c.Type == "culture");
                    if (existingCultureClaim != null)
                    {
                        await _userManager.RemoveClaimAsync(user, existingCultureClaim);
                    }
                    await _userManager.AddClaimAsync(user, new Claim("culture", culture));

                    await _signInManager.RefreshSignInAsync(user);
                }

                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );
            }
            else
            {
                Response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);
            }

            return LocalRedirect(returnUrl);
        }
    }
}
