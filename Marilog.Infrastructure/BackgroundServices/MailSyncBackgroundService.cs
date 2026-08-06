using Marilog.Application.Interfaces.Email;
using Marilog.Contracts.DTOs.Requests.EmailDTOs;
using Marilog.Contracts.DTOs.Requests.StoregFileDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.EmailServices;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Kernel.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marilog.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Runs inside the main API process (no separate worker project/deployment).
    /// Every SyncInterval: for each active EmailAccount, fetches new inbox
    /// messages and stores them as unlinked Email records ready for triage.
    /// </summary>
    public class MailSyncBackgroundService : BackgroundService
    {
        private static readonly TimeSpan SyncInterval = TimeSpan.FromMinutes(3);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MailSyncBackgroundService> _logger;

        public MailSyncBackgroundService(IServiceScopeFactory scopeFactory, ILogger<MailSyncBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SyncAllAccountsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Mail sync cycle failed.");
                }

                await Task.Delay(SyncInterval, stoppingToken);
            }
        }

        private async Task SyncAllAccountsAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var accountService = scope.ServiceProvider.GetRequiredService<IEmailAccountService>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var clientFactory = scope.ServiceProvider.GetRequiredService<IEmailProviderClientFactory>();
            var storedFileService = scope.ServiceProvider.GetRequiredService<IStoredFileService>();

            var accounts = await accountService.GetAllAsync(ct);

            foreach (var account in accounts.Where(a => a.IsActive))
            {
                try
                {
                    await SyncAccountAsync(account, accountService, emailService, clientFactory, storedFileService, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed syncing account {AccountId} ({Email})",
                        account.Id, account.EmailAddress);
                }
            }
        }

        private async Task SyncAccountAsync(
            EmailAccountResponse account,
            IEmailAccountService accountService,
            IEmailService emailService,
            IEmailProviderClientFactory clientFactory,
            IStoredFileService storedFileService,
            CancellationToken ct)
        {
            var config = await accountService.GetDecryptedConfigAsync(account.Id, ct);

            var originalConfig = new Dictionary<string, string>(config);

            var client = clientFactory.GetClient(account.ProviderType);
            
            var since = account.LastSyncedAt ?? DateTime.UtcNow.AddDays(-7);// First-ever sync: only look back 7 days, not the entire mailbox history.

            var messages = await client.FetchNewMessagesAsync(config, since, ct);

            var sentMessages = await client.FetchSentMessagesAsync(config, since, ct);

            foreach (var message in messages)
            {
                var email = await emailService.CreateFromInboundAsync(account.Id, message, ct);

                if (message.Attachments.Count > 0 && !await HasAttachmentsAsync(email.Id, message.Attachments, storedFileService, ct))
                    await UploadAttachmentsAsync(email.Id, message.Attachments, storedFileService, ct);
            }

            foreach (var message in sentMessages)
            {
                var email = await emailService.CreateFromSentAsync(account.Id, message, ct);
                if (message.Attachments.Count > 0 && !await HasAttachmentsAsync(email.Id, message.Attachments, storedFileService, ct))
                    await UploadAttachmentsAsync(email.Id, message.Attachments, storedFileService, ct);
            }


            if (ConfigChanged(config, originalConfig))
            {
                await accountService.UpdateConfigAsync(account.Id, config, ct);
            }
            await accountService.MarkSyncedAsync(account.Id, DateTime.UtcNow, ct);
        }
        private static async Task<bool> HasAttachmentsAsync(int emailId, List<InboundAttachment> incoming,
                                                      IStoredFileService storedFileService, CancellationToken ct)
        {
            var existing = await storedFileService.GetByEntityIdAsync(emailId, EntityType.Email, ct);
            return existing.Count >= incoming.Count;
        }
        private static async Task UploadAttachmentsAsync(int emailId, List<InboundAttachment> attachments, 
                                                         IStoredFileService storedFileService, CancellationToken ct)
        {
            var streams = new List<MemoryStream>();

            try
            {
                var requests = new List<UploadFileRequest>();

                foreach (var attachment in attachments)
                {
                    var stream = new MemoryStream(attachment.Content);
                    streams.Add(stream);

                    requests.Add(new UploadFileRequest
                    {
                        FileStream = stream,
                        FileName = attachment.FileName,
                        ContentType = attachment.ContentType,
                        Size = attachment.Content.LongLength,
                        EntityType = EntityType.Email,
                        EntityId = emailId
                    });
                }

                await storedFileService.UploadAsync(requests, ct);
            }
            finally
            {
                foreach (var stream in streams)
                    await stream.DisposeAsync();
            }
        }


        private static bool ConfigChanged(Dictionary<string, string> current, Dictionary<string, string> original)
        {
            if (current.Count != original.Count)
                return true;

            foreach (var pair in current)
            {
                if (!original.TryGetValue(pair.Key, out var value) ||
                    !string.Equals(value, pair.Value, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}