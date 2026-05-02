using Domain.Database;
namespace TaskJect.Web.Models
{
    public class AnalysisUserDetails
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserSurname { get; set; }
        public List<AnalyticsUserDetils> Tasks { get; set; }
        public Dictionary<Guid, string> Projects { get; set; }
        public string DateTo { get; set; }
    }
}