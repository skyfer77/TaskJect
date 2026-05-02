namespace Domain.Database
{
    public class AnalyticsUserTableModel
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public int CountTask { get; set; }
        public int TaskOverdue { get; set; }
        public int Points { get; set; }
        public int ActualHours { get; set; }
        public int ActualMinutes { get; set; }
    }
}
