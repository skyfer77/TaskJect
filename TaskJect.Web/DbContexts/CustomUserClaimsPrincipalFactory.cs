using Domain.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace TaskJect.Web.DbContexts
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public CustomUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("organization_code", user.OrganizationCode));

			var firstLetter = !string.IsNullOrEmpty(user.Name)
	            ? char.ToUpper(user.Name[0]).ToString()
	            : null;

			var avatarUrl = firstLetter != null
				? $"/images/default-avatars/{firstLetter}.png"
				: "/images/default-avatars/default-avatar.png";

			identity.AddClaim(new Claim("avatar", avatarUrl));

			if (!string.IsNullOrEmpty(user.Culture))
            {
                identity.AddClaim(new Claim("culture", user.Culture));
            }

            return identity;
        }
    }
}
