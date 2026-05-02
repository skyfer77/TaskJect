namespace Domain.Database
{
    public class TaskDetails
    {
        public string UserId { get; set; }
        public List<int> TasksCount { get; set; }
        public List<int> TasksPoint { get; set; }
    }
}
