using Domain.Database;
namespace TaskJect.Web.Models
{
    public class OrganizationViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Picture { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public string CurrentPlanCode { get; set; }
        public DateTime CurrentPlanDateTo { get; set; }
        public int CountOfParticipants { get; set; }
        public ApplicationUserLiteDto? TeamLead { get; set; }
    }
}
