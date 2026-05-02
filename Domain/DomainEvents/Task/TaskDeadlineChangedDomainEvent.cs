namespace Domain.DomainEvents
{
    public class TaskDeadlineChangedDomainEvent : IDomainEvent
    {
        public Guid TaskId { get; }
        public DateTime? OldDeadline { get; }
        public DateTime? NewDeadline { get; }

        public TaskDeadlineChangedDomainEvent(Guid taskId, DateTime? oldDeadline, DateTime newDeadline)
        {
            TaskId = taskId;
            OldDeadline = oldDeadline;
            NewDeadline = newDeadline;
        }
    }

}
