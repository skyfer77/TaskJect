using TaskJect.Web.Enums;

namespace TaskJect.Web.Models
{
    public class OrganizationAppealViewModel
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Picture { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public AppealStatusView Status { get; set; }
        public string? DescriptionRejecting { get; set; }
    }
}
