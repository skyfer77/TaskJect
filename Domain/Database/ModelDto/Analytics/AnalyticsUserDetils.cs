namespace Domain.Database
{
    public class AnalyticsUserDetils
    {
        public Guid? ID { get; set; }
        public Guid ProjectID { get; set; }
        public string Title { get; set; }
        public bool IsAgreedOverdue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? ComplitedDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public int Complexity { get; set; }
        public int? ActualHours { get; set; }
        public int? ActualMinutes { get; set; }
    }
}
