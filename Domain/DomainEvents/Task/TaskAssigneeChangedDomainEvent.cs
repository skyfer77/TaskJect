using Domain.DomainEvents;

namespace Domain.Handlers
{
    public class TaskAssigneeChangedDomainEvent : IDomainEvent
    {
        public Guid TaskId { get; }
        public Guid? OldAssigneeId { get; }
        public Guid NewAssigneeId { get; }

        public TaskAssigneeChangedDomainEvent(Guid taskId, Guid? oldAssigneeId, Guid newAssigneeId)
        {
            TaskId = taskId;
            OldAssigneeId = oldAssigneeId;
            NewAssigneeId = newAssigneeId;
        }
    }
}
