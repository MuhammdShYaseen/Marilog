using Marilog.Domain.Common;

namespace Marilog.Application.Interfaces.Events
{
    public interface IEventDispatcher 
    {
        Task EnqueueAsync(IDomainEvent @event, CancellationToken ct = default); 
    }
}
