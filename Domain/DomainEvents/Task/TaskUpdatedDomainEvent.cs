namespace Domain.DomainEvents
{
    public class TaskUpdatedDomainEvent : IDomainEvent
    {
        public Guid TaskId { get; }

        public TaskUpdatedDomainEvent(Guid taskId)
        {
            TaskId = taskId;
        }
    }
}
