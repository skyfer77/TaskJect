using TaskJect.Web.Enums;
using Domain.Database;

namespace TaskJect.Web.Models
{
    public class AnalysisUsersModel
    {
        public List<ApplicationUserLiteDto> Users { get; set; }
        public Dictionary<string, List<int>> Tasks { get; set; }
        public Dictionary<string, List<int>> Points { get; set; }
        public Dictionary<string, int> OverdueTasks { get; set; }
        public PeriodView TasksPeriod { get; set; }
        public QuickFilterView? QuickFilter { get; set; }
        public string DateTo { get; set; }
    }
}
