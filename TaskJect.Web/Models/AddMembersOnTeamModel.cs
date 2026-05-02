using Domain.Database;
namespace TaskJect.Web.Models
{
    public class ManageTeamModel
    {
        public Guid IdTeam { get; set; }
        public string Name { get; set; }
        public string OrganizationCode { get; set; }
        public List<MembershipDto> Membership { get; set; }
        public List<ApplicationUserLiteDto> User { get; set; }
    }
}
