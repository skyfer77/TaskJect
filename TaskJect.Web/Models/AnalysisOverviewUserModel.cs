using Domain.Enums;
using Domain.Database;

namespace TaskJect.Web.Models
{
    public class AnalysisOverviewUserModel
    {
        public ApplicationUserLiteDto User { get; set; }
        public int TaskCount { get; set; }
        public int PointSum { get; set; }
        public List<int> Tasks { get; set; }
        public List<int> Points { get; set; }
        public Period TasksPeriod { get; set; }
        public QuickFilter? QuickFilter { get; set; }
        public string DateTo { get; set; }
        public int CountTaskOverdue { get; set; }
    }
}
