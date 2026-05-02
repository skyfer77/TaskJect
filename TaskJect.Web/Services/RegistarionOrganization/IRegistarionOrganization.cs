using TaskJect.Web.Models;

namespace TaskJect.Web.Services
{
	public interface IRegistarionOrganization
	{
		Task<ServerResponse> RegistarionNewOrganization(RegisterViewModel model);
	}
}
