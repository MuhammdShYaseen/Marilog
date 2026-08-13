using MailKit.Net.Smtp;
using MailKit.Security;
using Marilog.Contracts.Interfaces.Services.EmailNotificationConfig;
using Marilog.Infrastructure.Interfaces.EmailNotification;
using Marilog.Infrastructure.Models.EmailNotification;
using MimeKit;

namespace Marilog.Infrastructure.Services.EmailNotification
{
    public sealed class NotificationEmailSender : INotificationEmailSender
    {
        private readonly INotificationSenderEmailSettingsStore _settingsStore;

        public NotificationEmailSender(INotificationSenderEmailSettingsStore settingsStore)
        {
            _settingsStore = settingsStore;
        }

        public async Task SendAsync(NotificationEmailMessage message, IReadOnlyCollection<string> recipients, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);
            ArgumentNullException.ThrowIfNull(recipients);

            if (recipients.Count == 0)
                return;

            var settings = await _settingsStore.GetAsync(cancellationToken);

            var email = new MimeMessage();

            email.From.Add(
                new MailboxAddress(
                    message.FromName ?? string.Empty,
                    message.FromEmail));

            foreach (var recipient in recipients)
            {
                email.To.Add(
                    MailboxAddress.Parse(recipient));
            }

            email.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message.HtmlBody
            };

            email.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            var socketOption = settings.SmtpPort switch
            {
                465 => SecureSocketOptions.SslOnConnect,
                587 => SecureSocketOptions.StartTls,
                _ => SecureSocketOptions.Auto
            };

            await client.ConnectAsync(settings.SmtpHost, settings.SmtpPort, socketOption, cancellationToken);

            await client.AuthenticateAsync(settings.Username, settings.Password, cancellationToken);

            await client.SendAsync(email, cancellationToken);

            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}