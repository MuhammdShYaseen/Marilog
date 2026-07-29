using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Marilog.Application.Interfaces.Email;
using Marilog.Contracts.DTOs.Requests.EmailDTOs;
using MimeKit;

namespace Marilog.Infrastructure.Services
{
    /// <summary>
    /// Generic IMAP (receive) / SMTP (send) client — works with any provider that
    /// exposes standard IMAP/SMTP, using a username + password (or app password).
    /// Expected config keys: imapHost, imapPort, smtpHost, smtpPort, username,
    /// password, useSsl ("true"/"false").
    /// </summary>
    public class ImapSmtpEmailProviderClient : IEmailProviderClient
    {
        public async Task<IReadOnlyList<InboundMessage>> FetchNewMessagesAsync(
            Dictionary<string, string> config, DateTime since, CancellationToken ct = default)
        {
            return await FetchFromFolderAsync(config, since,
                (client, ct) => Task.FromResult(client.Inbox), ct);
        }

        public async Task<IReadOnlyList<InboundMessage>> FetchSentMessagesAsync(
            Dictionary<string, string> config, DateTime since, CancellationToken ct = default)
        {
            return await FetchFromFolderAsync(config, since,
                (client, ct) => GetSentFolderAsync(client, config, ct), ct);
        }

        private static async Task<IReadOnlyList<InboundMessage>> FetchFromFolderAsync(
            Dictionary<string, string> config, DateTime since,
            Func<ImapClient, CancellationToken, Task<IMailFolder>> resolveFolder,
            CancellationToken ct)
        {
            var host = config["imapHost"];
            var port = int.Parse(config["imapPort"]);
            var username = config["username"];
            var password = config["password"];
            var useSsl = !config.TryGetValue("useSsl", out var uv) || bool.Parse(uv);

            using var client = new ImapClient();
            await client.ConnectAsync(host, port, useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(username, password, ct);

            var folder = await resolveFolder(client, ct);
            await folder.OpenAsync(FolderAccess.ReadOnly, ct);

            var uids = await folder.SearchAsync(SearchQuery.DeliveredAfter(since), ct);

            var messages = new List<InboundMessage>(uids.Count);

            foreach (var uid in uids)
            {
                var mime = await folder.GetMessageAsync(uid, ct);
                messages.Add(MapToInboundMessage(mime));
            }

            await client.DisconnectAsync(true, ct);
            return messages;
        }

        /// <summary>
        /// Resolves the Sent folder. Priority: config["sentFolderName"] if the
        /// caller knows the exact name (needed for servers with no SPECIAL-USE
        /// support) → IMAP SPECIAL-USE flag (works on most modern servers,
        /// including Gmail/Graph-backed IMAP) → a list of common folder names
        /// as a last resort.
        /// </summary>
        private static async Task<IMailFolder> GetSentFolderAsync(
            ImapClient client, Dictionary<string, string> config, CancellationToken ct)
        {
            var personal = client.GetFolder(client.PersonalNamespaces[0]);

            if (config.TryGetValue("sentFolderName", out var configuredName))
            {
                var configured = await TryGetSubfolderAsync(personal, configuredName, ct);
                if (configured is not null) return configured;
            }

            try
            {
                return client.GetFolder(SpecialFolder.Sent) ?? throw new NullReferenceException("GetFolder == null");
            }
            catch (NotSupportedException) { /* server has no SPECIAL-USE — fall through */ }
            catch (FolderNotFoundException) { /* server didn't flag one — fall through */ }

            var candidates = new[] { "Sent", "Sent Items", "Sent Mail", "[Gmail]/Sent Mail" };

            foreach (var name in candidates)
            {
                var folder = await TryGetSubfolderAsync(personal, name, ct);
                if (folder is not null) return folder;
            }

            throw new FolderNotFoundException(
                "Could not resolve a Sent folder via config[\"sentFolderName\"], " +
                "SPECIAL-USE, or common names. Set config[\"sentFolderName\"] explicitly for this account.");
        }

        private static async Task<IMailFolder?> TryGetSubfolderAsync(IMailFolder parent, string name, CancellationToken ct)
        {
            try
            {
                return await parent.GetSubfolderAsync(name, ct);
            }
            catch (FolderNotFoundException)
            {
                return null;
            }
        }

        public async Task<string> SendAsync(
            Dictionary<string, string> config, string fromAddress, string? fromDisplayName,
            OutboundMessage message, CancellationToken ct = default)
        {
            var host = config["smtpHost"];
            var port = int.Parse(config["smtpPort"]);
            var username = config["username"];
            var password = config["password"];
            var useSsl = !config.TryGetValue("useSsl", out var uv) || bool.Parse(uv);

            var mime = new MimeMessage();
            mime.MessageId = MimeKit.Utils.MimeUtils.GenerateMessageId();
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

            return mime.MessageId;
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