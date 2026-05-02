using Domain.Database;
namespace TaskJect.Web.Models
{
    public class UsersInTheOrganizationModel
    {
        public OrganizationDto Organization { get; set; }
        public List<ApplicationUserLiteDto> Users { get; set; }
        public List<RoleInfoModel> Roles { get; set; }
    }
}
