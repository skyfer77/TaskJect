namespace Domain.Database
{
    public class AddHoursModel
    {
        public string TaskId { get; set; }
        public int? NewHours { get; set; }
        public int? NewMinutes { get; set;}
    }
}
