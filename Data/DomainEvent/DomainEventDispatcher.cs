using Domain.DomainEvents;
using Domain.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Data.DomainEvent
{
    public class DomainEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public DomainEventDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents)
        {
            foreach (var domainEvent in domainEvents)
            {
                var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
                var handlers = _serviceProvider.GetServices(handlerType);
                //var handlers = _serviceProvider.GetServices<IDomainEventHandler<IDomainEvent>>();

                foreach (var handler in handlers)
                {
                    var method = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync));
                    if (method != null)
                    {
                        await (Task)method.Invoke(handler, new object[] { domainEvent });
                    }
                }
            }
        }
    }

}
