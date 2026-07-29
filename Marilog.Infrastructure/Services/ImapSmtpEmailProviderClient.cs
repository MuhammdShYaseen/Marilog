
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Marilog.Contracts.DTOs.Requests.EmailDTOs;
using Marilog.Contracts.Interfaces.Services.EmailServices;
using MimeKit;
namespace Marilog.Infrastructure.Services
{
    public class ImapSmtpEmailProviderClient : IEmailProviderClient
    {
        public async Task<IReadOnlyList<InboundMessage>> FetchNewMessagesAsync(
            Dictionary<string, string> config, DateTime since, CancellationToken ct = default)
        {
            var host = config["imapHost"];
            var port = int.Parse(config["imapPort"]);
            var username = config["username"];
            var password = config["password"];
            var useSsl = !config.TryGetValue("useSsl", out var uv) || bool.Parse(uv);

            using var client = new ImapClient();
            await client.ConnectAsync(host, port, useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(username, password, ct);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly, ct);

            var uids = await inbox.SearchAsync(SearchQuery.DeliveredAfter(since), ct);

            var messages = new List<InboundMessage>(uids.Count);

            foreach (var uid in uids)
            {
                var mime = await inbox.GetMessageAsync(uid, ct);
                messages.Add(MapToInboundMessage(mime));
            }

            await client.DisconnectAsync(true, ct);
            return messages;
        }

        public async Task SendAsync(
            Dictionary<string, string> config, string fromAddress, string? fromDisplayName,
            OutboundMessage message, CancellationToken ct = default)
        {
            var host = config["smtpHost"];
            var port = int.Parse(config["smtpPort"]);
            var username = config["username"];
            var password = config["password"];
            var useSsl = !config.TryGetValue("useSsl", out var uv) || bool.Parse(uv);

            var mime = new MimeMessage();
            mime.From.Add(new MailboxAddress(fromDisplayName ?? fromAddress, fromAddress));

            foreach (var to in message.ToAddresses)
                mime.To.Add(MailboxAddress.Parse(to));
            foreach (var cc in message.CcAddresses)
                mime.Cc.Add(MailboxAddress.Parse(cc));

            mime.Subject = message.Subject;

            var builder = new BodyBuilder { HtmlBody = message.Body };
            foreach (var attachment in message.Attachments)
                builder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));

            mime.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, ct);
            await client.AuthenticateAsync(username, password, ct);
            await client.SendAsync(mime, ct);
            await client.DisconnectAsync(true, ct);
        }

        private static InboundMessage MapToInboundMessage(MimeMessage mime)
        {
            var from = mime.From.Mailboxes.FirstOrDefault();

            var inbound = new InboundMessage
            {
                ExternalId = mime.MessageId ?? Guid.NewGuid().ToString(),
                Subject = mime.Subject ?? string.Empty,
                Body = mime.HtmlBody ?? mime.TextBody ?? string.Empty,
                ReceivedAt = mime.Date.UtcDateTime,
                FromAddress = from?.Address ?? string.Empty,
                FromDisplayName = from?.Name,
                ToAddresses = mime.To.Mailboxes.Select(m => m.Address).ToList(),
                CcAddresses = mime.Cc.Mailboxes.Select(m => m.Address).ToList()
            };

            foreach (var attachment in mime.Attachments)
            {
                byte[] content;

                using (var ms = new MemoryStream())
                {
                    if (attachment is MimePart part && part.Content != null)
                        part.Content.DecodeTo(ms);
                    content = ms.ToArray();
                }

                inbound.Attachments.Add(new InboundAttachment
                {
                    FileName = attachment.ContentDisposition?.FileName
                               ?? (attachment as MimePart)?.FileName
                               ?? "attachment",
                    ContentType = attachment.ContentType?.MimeType ?? "application/octet-stream",
                    Content = content
                });
            }

            return inbound;
        }
    }
}
