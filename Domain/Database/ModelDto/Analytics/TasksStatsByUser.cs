using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Database
{
    public class TasksStatsByUser
    {
        public string UserId { get; set; }
        public Dictionary<DateTime, int> SumPoints { get; set; }
        public Dictionary<DateTime, int> SumCountTask { get; set; }
        public Dictionary<DateTime, int> SumTaskOverdue { get; set; }
        public Dictionary<DateTime, int> SumHours { get; set; }
        public Dictionary<DateTime, int> SumMinutes { get; set; }

        public TasksStatsByUser()
        {
            SumPoints = new Dictionary<DateTime, int>();
            SumCountTask = new Dictionary<DateTime, int>();
            SumTaskOverdue = new Dictionary<DateTime, int>();
            SumHours = new Dictionary<DateTime, int>();
            SumMinutes = new Dictionary<DateTime, int>();
        }
    }

    [NotMapped]
    public class TasksStatisticByPeriod
    {
        public string UserId { get; set; }
        public int SumPoints { get; set; }
        public int SumCountTask { get; set; }
        public int SumTaskOverdue { get; set; }
        public int SumHours { get; set; }
        public int SumMinutes { get; set; }

    }
}
