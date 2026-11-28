using Marilog.Application.Interfaces.Events;
using Marilog.Domain.Common;
using Marilog.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Marilog.Ship.Services.Files
{
    /// <summary>
    /// The ship's version of Marilog's InMemoryEventDispatcher + DomainEventProcessor:
    /// each event runs in the background in its own scope, after the save that raised it.
    /// Events with no handler on the ship (e.g. DocumentFullyPaidEvent) are ignored.
    /// </summary>
    public sealed class ShipEventDispatcher : IEventDispatcher
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ShipEventDispatcher> _logger;

        public ShipEventDispatcher(IServiceScopeFactory scopeFactory, ILogger<ShipEventDispatcher> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Task EnqueueAsync(IDomainEvent @event, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(@event);
            _ = Task.Run(() => DispatchAsync(@event));
            return Task.CompletedTask;
        }

        private async Task DispatchAsync(IDomainEvent @event)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();

                switch (@event)
                {
                    case StoredFileOcrRequestedEvent e:
                        var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<StoredFileOcrRequestedEvent>>();
                        await handler.HandleAsync(e);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling domain event {EventType}", @event.GetType().Name);
            }
        }
    }
}