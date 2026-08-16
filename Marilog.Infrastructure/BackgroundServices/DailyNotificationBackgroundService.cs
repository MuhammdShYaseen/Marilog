using Marilog.Contracts.DTOs.EmailNotificationDTOs;
using Marilog.Contracts.Interfaces.Services.EmailNotificationConfig;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Infrastructure.Helpers.EmailNotification;
using Marilog.Infrastructure.Interfaces.EmailNotification;
using Marilog.Infrastructure.Models.EmailNotification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
namespace Marilog.Infrastructure.BackgroundServices;

public sealed class DailyNotificationBackgroundService : BackgroundService
{
    private readonly ILogger<DailyNotificationBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public DailyNotificationBackgroundService(ILogger<DailyNotificationBackgroundService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _scopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daily notification background service started.");
        await using var scope = _scopeFactory.CreateAsyncScope();
        var _notificationSchedule = scope.ServiceProvider.GetRequiredService<INotificationSchedule>();
        var _scheduleChangeNotifier = scope.ServiceProvider.GetRequiredService<INotificationScheduleChangeNotifier>();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var nextExecution = await _notificationSchedule.GetNextExecutionAsync(DateTimeOffset.UtcNow, stoppingToken);

                if (nextExecution is null)
                {
                    await _scheduleChangeNotifier.WaitForChangeAsync(stoppingToken);

                    continue;
                }

                var delay = nextExecution.Value - DateTimeOffset.UtcNow;

                if (delay < TimeSpan.Zero)
                    delay = TimeSpan.Zero;

                _logger.LogInformation("Next daily notification execution is scheduled at {ExecutionTimeUtc}.", nextExecution.Value);

                var delayTask = Task.Delay(delay, stoppingToken);

                var changeTask = _scheduleChangeNotifier.WaitForChangeAsync(stoppingToken);

                var completedTask = await Task.WhenAny(delayTask, changeTask);

                if (completedTask == changeTask)
                {
                    _logger.LogInformation("Notification schedule changed. Recalculating execution time.");

                    continue;
                }

                if (stoppingToken.IsCancellationRequested)
                    break;

                await ProcessNotificationsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while processing daily notifications.");
            }
        }

        _logger.LogInformation("Daily notification background service stopped.");
    }

    private async Task ProcessNotificationsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting daily notification processing.");

        // ─────────────────────────────────────────────────────────────
        // 1. Sender email settings
        // ─────────────────────────────────────────────────────────────

        await using var scope = _scopeFactory.CreateAsyncScope();

        var _senderEmailSettingsStore = scope.ServiceProvider.GetRequiredService<INotificationSenderEmailSettingsStore>();

        var _recipientStore = scope.ServiceProvider.GetRequiredService<INotificationRecipientStore>();

        var _notificationSettingsStore = scope.ServiceProvider.GetRequiredService<INotificationSettingsStore>();

        var _emailSender = scope.ServiceProvider.GetRequiredService<INotificationEmailSender>();

        var senderSettings = await _senderEmailSettingsStore.GetAsync(cancellationToken);

        if (!IsValidSenderEmailSettings(senderSettings))
        {
            _logger.LogWarning("Daily notification processing skipped because sender email settings are missing or invalid.");
            return;
        }

        // ─────────────────────────────────────────────────────────────
        // 2. Recipients
        // ─────────────────────────────────────────────────────────────

        var recipients = await _recipientStore.GetAllAsync(cancellationToken);

        var validRecipients = recipients
            .Where(IsValidEmail)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (validRecipients.Length == 0)
        {
            _logger.LogWarning("Daily notification processing skipped because no valid notification recipients were configured.");

            return;
        }

        // ─────────────────────────────────────────────────────────────
        // 3. Notification settings
        // ─────────────────────────────────────────────────────────────

        var notificationSettings = await _notificationSettingsStore.GetAsync(cancellationToken);

        if (!HasEnabledNotificationSettings(notificationSettings))
        {
            _logger.LogInformation("Daily notification processing skipped because no notification types are enabled.");

            return;
        }

        // ─────────────────────────────────────────────────────────────
        // 4. Collect notification data
        // ─────────────────────────────────────────────────────────────

        var notificationData = await CollectNotificationDataAsync(notificationSettings, cancellationToken);

        if (!HasNotificationData(notificationData))
        {
            _logger.LogInformation("Daily notification processing completed. No notification data was found.");

            return;
        }

        // ─────────────────────────────────────────────────────────────
        // 5. Build and send email
        // ─────────────────────────────────────────────────────────────

        var email = BuildNotificationEmail(senderSettings, notificationData);

        await _emailSender.SendAsync(email, validRecipients, cancellationToken);

        _logger.LogInformation("Daily notification email sent successfully to {RecipientCount} recipient(s).", validRecipients.Length);
    }

    private async Task<NotificationDataResponse> CollectNotificationDataAsync(NotificationSettingsOptions settings, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var _documentService = scope.ServiceProvider.GetRequiredService<IDocumentService>();
        var _personService = scope.ServiceProvider.GetRequiredService<IPersonService>();
        var _vesselService = scope.ServiceProvider.GetRequiredService<IVesselService>();
        var response = new NotificationDataResponse();

        if (settings.UnpaidDocuments)
        {
            var documents = await _documentService.GetUnpaidAsync(false, cancellationToken);

            response.UnpaidDocuments = documents;
        }

        if (settings.ExpiringVesselCertificates)
        {
            var certificates = await _vesselService.GetExpiringCertificates(cancellationToken);

            response.ExpiringVesselCertificates = certificates;
            
        }

        if (settings.ExpiredVesselCertificates)
        {
            var certificates = await _vesselService.GetExpiredCertificates(cancellationToken);

            response.ExpiredVesselCertificates = certificates;
        }

        if (settings.ExpiringPersonCertificates)
        {
            var certificates = await _personService.GetExpiringCertificates(cancellationToken);

            response.ExpiringPersonCertificates = certificates;
            
        }

        if (settings.ExpiredPersonCertificates)
        {
            var certificates = await _personService.GetExpiredCertificates(cancellationToken);


            response.ExpiredPersonCertificates = certificates;

        }

        return response;
    }

    private static NotificationEmailMessage BuildNotificationEmail(NotificationSenderEmailSettingsOptions senderSettings, NotificationDataResponse data)
    {
        var body = NotificationEmailBuilder.Build(data);

        return new NotificationEmailMessage
        {
            FromEmail = senderSettings.FromEmail,
            FromName = senderSettings.FromName,
            Subject = $"Daily Notifications - {DateTime.UtcNow:yyyy-MM-dd}",
            HtmlBody = body
        };
    }

    private static bool IsValidSenderEmailSettings(NotificationSenderEmailSettingsOptions options)
    {
        return
            !string.IsNullOrWhiteSpace(options.SmtpHost) &&
            options.SmtpPort > 0 &&
            !string.IsNullOrWhiteSpace(options.ImapHost) &&
            options.ImapPort > 0 &&
            !string.IsNullOrWhiteSpace(options.Username) &&
            !string.IsNullOrWhiteSpace(options.Password) &&
            !string.IsNullOrWhiteSpace(options.FromEmail) &&
            IsValidEmail(options.FromEmail);
    }

    private static bool HasEnabledNotificationSettings(NotificationSettingsOptions settings)
    {
        return
            settings.UnpaidDocuments ||
            settings.ExpiringVesselCertificates ||
            settings.ExpiredVesselCertificates ||
            settings.ExpiringPersonCertificates ||
            settings.ExpiredPersonCertificates;
    }

    private static bool HasNotificationData(NotificationDataResponse data)
    {
        return
            data.UnpaidDocuments.Count > 0 ||
            data.ExpiringVesselCertificates.Count > 0 ||
            data.ExpiredVesselCertificates.Count > 0 ||
            data.ExpiringPersonCertificates.Count > 0 ||
            data.ExpiredPersonCertificates.Count > 0;
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var address = new MailAddress(email);

            return string.Equals(
                address.Address,
                email.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
   
}
