using Domain.DomainEvents;

namespace Domain.Handlers
{
    public interface IDomainEventHandler<in TDomainEvent> where TDomainEvent : IDomainEvent
    {
        bool CanHandle(IDomainEvent domainEvent);
        Task HandleAsync(IDomainEvent domainEvent);
    }
}
