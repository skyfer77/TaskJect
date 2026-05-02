using Domain.Enums;

namespace Domain.Database
{
    public class TaskStatisticsRequest
    {
        public List<string> UserIds { get; set; }
        public Period TasksPeriod { get; set; } = Period.Week;
        public QuickFilter QuickFilter { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string OrganizationCode { get; set; }
	}
}
