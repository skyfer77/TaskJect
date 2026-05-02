using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace TaskJect.Web.Middleware
{
    public class OrgCodeValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public OrgCodeValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;

            if (!path.StartsWithSegments("/Account", StringComparison.OrdinalIgnoreCase))
            {
                var user = context.User;
                var isAuthenticated = user.Identity?.IsAuthenticated ?? false;

                if (isAuthenticated)
                {
                    var orgCode = user.FindFirst("organization_code")?.Value;

                    if (string.IsNullOrEmpty(orgCode))
                    {
                        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
                        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                        var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
                        context.Response.Cookies.Delete(".AspNetCore.Identity.Application");
                        context.Response.Cookies.Delete("ByteBustersCookies" + env.EnvironmentName);

                        context.User = new ClaimsPrincipal(new ClaimsIdentity());

                        context.Response.Redirect("/Account/Login");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

    public static class OrgCodeValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseOrgCodeValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<OrgCodeValidationMiddleware>();
        }
    }

}
