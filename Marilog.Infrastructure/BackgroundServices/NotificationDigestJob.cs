using Marilog.Application.Services.ApplicationServices.NotifiServices;
using Marilog.Contracts.DTOs.EmailNotificationDTOs;
using Marilog.Contracts.Interfaces.Services.EmailNotificationConfig;
using Marilog.Domain.Entities.NotifiEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Infrastructure.Helpers.EmailNotification;
using Marilog.Infrastructure.Helpers.UserIdentity;
using Marilog.Infrastructure.Interfaces.EmailNotification;
using Marilog.Infrastructure.Models.EmailNotification;
using Marilog.Infrastructure.Models.Identity;
using Marilog.Kernel.Enumerations;
using Marilog.Kernel.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace Marilog.Infrastructure.BackgroundServices;

// At the scheduled time each day:
// 1. due dates and certificate expiries of followed records → bell
// 2. each user gets one email with their notifications not emailed yet
public sealed class NotificationDigestJob : BackgroundService
{
    private readonly ILogger<NotificationDigestJob> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;

    public NotificationDigestJob(ILogger<NotificationDigestJob> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification digest job started.");
        await using var scope = _scopeFactory.CreateAsyncScope();
        var schedule = scope.ServiceProvider.GetRequiredService<INotificationSchedule>();
        var scheduleChangeNotifier = scope.ServiceProvider.GetRequiredService<INotificationScheduleChangeNotifier>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var nextExecution = await schedule.GetNextExecutionAsync(DateTimeOffset.UtcNow, stoppingToken);

                if (nextExecution is null)
                {
                    await scheduleChangeNotifier.WaitForChangeAsync(stoppingToken);
                    continue;
                }

                var delay = nextExecution.Value - DateTimeOffset.UtcNow;
                if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;

                _logger.LogInformation("Next notification digest is scheduled at {ExecutionTimeUtc}.", nextExecution.Value);

                var delayTask = Task.Delay(delay, stoppingToken);
                var changeTask = scheduleChangeNotifier.WaitForChangeAsync(stoppingToken);

                if (await Task.WhenAny(delayTask, changeTask) == changeTask)
                {
                    _logger.LogInformation("Notification schedule changed. Recalculating execution time.");
                    continue;
                }

                if (stoppingToken.IsCancellationRequested) break;

                await ProcessAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification digest run failed.");
            }
        }

        _logger.LogInformation("Notification digest job stopped.");
    }

    private async Task ProcessAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var sp = scope.ServiceProvider;

        // 1. Due dates and expiries → bell
        var due = await sp.GetRequiredService<DueNotificationService>()
            .CreateAsync(DateOnly.FromDateTime(DateTime.UtcNow), ct);
        _logger.LogInformation("Due notifications created: {Count}.", due);

        // 2. Email
        var senderSettings = await sp.GetRequiredService<INotificationSenderEmailSettingsStore>().GetAsync(ct);
        if (!IsValidSenderEmailSettings(senderSettings))
        {
            _logger.LogWarning("Notification emails skipped because sender email settings are missing or invalid.");
            return;
        }

        var repo = sp.GetRequiredService<INotificationRepository>();
        var pending = await repo.Notifications()
            .Where(n => n.EmailedAt == null)
            .OrderBy(n => n.Id)
            .ToListAsync(ct);

        if (pending.Count == 0) return;

        var userIds = pending.Select(n => n.UserId).Distinct().ToList();
        var users = await sp.GetRequiredService<UserManager<User>>().Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync(ct);

        var permissionReader = sp.GetRequiredService<UserPermissionReader>();
        var sender = sp.GetRequiredService<INotificationEmailSender>();
        var appUrl = _configuration["DailyNotification:AppUrl"];
        var emailsSent = 0;

        foreach (var group in pending.GroupBy(n => n.UserId))
        {
            var user = users.FirstOrDefault(u => u.Id == group.Key);
            var items = new List<Notification>();

            // Unread ones, of types the user can still read (same rule as the bell)
            if (user is { IsActive: true } && IsValidEmail(user.Email))
            {
                var permissions = await permissionReader.GetAsync(user, ct);
                items = group
                    .Where(n => n.ReadAt == null &&
                                permissions.Can(PermissionTargetKind.Entity, (int)PermissionCatalog.PermissionOwner(n.EntityType), PermissionAction.Read))
                    .ToList();
            }

            if (items.Count > 0)
            {
                var message = new NotificationEmailMessage
                {
                    FromEmail = senderSettings.FromEmail,
                    FromName = senderSettings.FromName,
                    Subject = $"Marilog: {items.Count} new notification{(items.Count == 1 ? "" : "s")}",
                    HtmlBody = NotificationEmailBuilder.Build(user!.FullName, items, appUrl)
                };

                try
                {
                    await sender.SendAsync(message, new[] { user.Email! }, ct);
                    emailsSent++;
                }
                catch (Exception ex)
                {
                    // Not marked: they go out with the next run
                    _logger.LogError(ex, "Notification email to user {UserId} failed.", user.Id);
                    continue;
                }
            }

            // Read, unreadable or no email: marked too, so they never pile up
            foreach (var n in group) n.MarkEmailed();
        }

        await repo.SaveChangesAsync(ct);
        _logger.LogInformation("Notification emails sent: {Count}.", emailsSent);
    }

    private static bool IsValidSenderEmailSettings(NotificationSenderEmailSettingsOptions options) =>
        !string.IsNullOrWhiteSpace(options.SmtpHost) &&
        options.SmtpPort > 0 &&
        !string.IsNullOrWhiteSpace(options.ImapHost) &&
        options.ImapPort > 0 &&
        !string.IsNullOrWhiteSpace(options.Username) &&
        !string.IsNullOrWhiteSpace(options.Password) &&
        IsValidEmail(options.FromEmail);

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        try
        {
            var address = new MailAddress(email);
            return string.Equals(address.Address, email.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}