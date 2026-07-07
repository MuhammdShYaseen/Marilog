using Marilog.Application.Interfaces.Events;
using Marilog.Domain.Common;
using Marilog.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marilog.Infrastructure.Dispatchers;

public sealed class DomainEventProcessor : BackgroundService
{
    private readonly InMemoryEventDispatcher _dispatcher;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DomainEventProcessor> _logger;

    public DomainEventProcessor(
        InMemoryEventDispatcher dispatcher,
        IServiceScopeFactory scopeFactory,
        ILogger<DomainEventProcessor> logger)
    {
        _dispatcher = dispatcher;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var @event in _dispatcher.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = _scopeFactory.CreateScope();

            try
            {
                await DispatchToHandlerAsync(@event, scope.ServiceProvider, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling domain event {EventType}", @event.GetType().Name);
            }
        }
    }

    private static async Task DispatchToHandlerAsync(
        IDomainEvent @event,
        IServiceProvider provider,
        CancellationToken ct)
    {
        // لازم switch/if صريح لأنه النوع الديناميكي معروف فقط وقت التنفيذ
        switch (@event)
        {
            case StoredFileOcrRequestedEvent e:
                var handler = provider.GetRequiredService<IEventHandler<StoredFileOcrRequestedEvent>>();
                await handler.HandleAsync(e, ct);
                break;

            // أضف case جديد لكل نوع domain event جديد بالمستقبل
            default:
                throw new InvalidOperationException(
                    $"No handler mapping registered for event type {@event.GetType().Name}");
        }
    }
}