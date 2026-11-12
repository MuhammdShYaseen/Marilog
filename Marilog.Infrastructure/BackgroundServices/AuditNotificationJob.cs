using Marilog.Application.Services.ApplicationServices.NotifiServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marilog.Infrastructure.BackgroundServices;

// Every minute: new AuditLog rows → notifications for followers.
public sealed class AuditNotificationJob : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditNotificationJob> _logger;

    public AuditNotificationJob(IServiceScopeFactory scopeFactory, ILogger<AuditNotificationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        long? cursor = null;
        using var timer = new PeriodicTimer(Interval);

        do
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<AuditNotificationService>();

                cursor ??= await service.GetStartCursorAsync(stoppingToken);
                cursor = await service.ProcessAsync(cursor.Value, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // The cursor stays: the same logs are tried again next minute
                _logger.LogError(ex, "Audit notifications run failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}