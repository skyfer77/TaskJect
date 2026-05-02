using Domain.Enums;

namespace Domain.Database
{ 
    public class OrganizationAppealDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationCode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public AppealStatus Status { get; set; }
        public string? DescriptionRejecting { get; set; }
    }
}
