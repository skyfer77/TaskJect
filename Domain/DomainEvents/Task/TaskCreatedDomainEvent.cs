namespace Domain.DomainEvents
{
    public class TaskCreatedDomainEvent : IDomainEvent
    {
        public Guid TaskId { get; }

        public TaskCreatedDomainEvent(Guid taskId)
        {
            TaskId = taskId;
        }
    }
}
