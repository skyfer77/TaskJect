using Domain.Database;
namespace TaskJect.Web.Models
{
    public class TeamPageModel
    {
        public List<ApplicationUserLiteDto> Users { get; set; }
        public IEnumerable<MembershipDto> Memberships { get; set; }
        public IEnumerable<TeamDto> Teams { get; set; }
        public Dictionary<TeamDto, List<ApplicationUserLiteDto>> TeamsWithUsers { get; set; }
        public Dictionary<string, TasksCountByUser> UsersWithCompletedTasks { get; set; }
    }
}
