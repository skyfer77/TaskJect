namespace Domain.DomainEvents
{
    public class TaskStatusChangedDomainEvent : IDomainEvent
    {
        public Guid TaskId { get; }
        public Enums.TaskStatus OldStatus { get; }
        public Enums.TaskStatus NewStatus { get; }

        public TaskStatusChangedDomainEvent(Guid taskId, Enums.TaskStatus oldStatus, Enums.TaskStatus newStatus)
        {
            TaskId = taskId;
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
    }
}
