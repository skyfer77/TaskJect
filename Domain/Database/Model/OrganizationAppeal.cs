using Domain.DomainEvents;
using Domain.Enums;

namespace Domain.Database
{
    public class OrganizationAppeal : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid OrganizationCode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public AppealStatus Status { get; set; }
        public string? DescriptionRejecting { get; set; }

        public void MarkAsSended()
        {
            AddDomainEvent(new OrganizationAppealSendsDomainEvent(Id, OrganizationCode));
        }
    }
}
