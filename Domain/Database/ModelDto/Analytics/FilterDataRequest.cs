using Domain.Database;
using Domain.Enums;

namespace TaskJect.Web.Models
{
    public class FilterDataRequest
    {
        public QuickFilter QuickFilter { get; set; }
        public string DateTo { get; set; }
        public Period Period { get; set; }
        public Func<TasksStatsByUser, Dictionary<DateTime, int>> GetStatsDictionary { get; set; }
    }
}
