using TaskJect.Web.Models;
namespace TaskJect.Web.Services
{
    public interface IAvailableProjectPermissionChecker
    {
        Task<ProjectPermissionModel> Check(Guid projectId, string userId);
    }
}
