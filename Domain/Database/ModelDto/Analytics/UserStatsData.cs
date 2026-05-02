using Domain.Enums;

namespace Domain.Database
{
    public class UserStatsData
    {
        public List<ApplicationUserLiteDto> Users { get; set; }
        public List<TasksStatsByUser> Stats { get; set; }
        public QuickFilter QuickFilter { get; set; }
        public string? DateTo { get; set; }
    }
}
