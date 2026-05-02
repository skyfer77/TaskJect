namespace Domain.DomainEvents
{
    public abstract class BaseEntity
    {
        public List<IDomainEvent> DomainEvents { get; } = new();

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            DomainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            DomainEvents.Clear();
        }
    }
}
