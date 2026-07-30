using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Marilog.Application.Interfaces.Email;
using Marilog.Contracts.DTOs.Requests.EmailDTOs;
using Marilog.Infrastructure.Models.Email;
using MimeKit;
using MimeKit.Utils;
using System.Text;
using MessagePart = Google.Apis.Gmail.v1.Data.MessagePart;

namespace Marilog.Infrastructure.Services.Email.Google
{
    public sealed class GoogleApiEmailProviderClient : IEmailProviderClient
    {
        private readonly IGoogleOAuthTokenService _tokenService;
        

        public GoogleApiEmailProviderClient(IGoogleOAuthTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task<IReadOnlyList<InboundMessage>> FetchNewMessagesAsync(Dictionary<string, string> config, DateTime since, CancellationToken ct = default)
        {
            await EnsureTokenAsync(config, ct);

            var service = CreateGmailService(config);
            var query = BuildDateQuery(since);

            var messageIds = await ListMessageIdsAsync(service, query, ct);
            var result = new List<InboundMessage>(messageIds.Count);

            foreach (var id in messageIds)
            {
                ct.ThrowIfCancellationRequested();

                var message = await GetMessageAsync(service, id, ct);
                if (message is null)
                    continue;

                var mapped = await MapMessageAsync(service, message, ct);
                result.Add(mapped);
            }

            return result;
        }

        public async Task<IReadOnlyList<InboundMessage>> FetchSentMessagesAsync(Dictionary<string, string> config, DateTime since, CancellationToken ct = default)
        {
            await EnsureTokenAsync(config, ct);

            var service = CreateGmailService(config);
            var query = $"in:sent {BuildDateQuery(since)}";

            var messageIds = await ListMessageIdsAsync(service, query, ct);
            var result = new List<InboundMessage>(messageIds.Count);

            foreach (var id in messageIds)
            {
                ct.ThrowIfCancellationRequested();

                var message = await GetMessageAsync(service, id, ct);
                if (message is null)
                    continue;

                result.Add(await MapMessageAsync(service, message, ct));
            }

            return result;
        }

        public async Task<string> SendAsync(Dictionary<string, string> config, string fromAddress, string? fromDisplayName, OutboundMessage message, CancellationToken ct = default)
        {
            await EnsureTokenAsync(config, ct);

            var service = CreateGmailService(config);

            var mime = BuildMimeMessage(fromAddress, fromDisplayName, message);

            using var stream = new MemoryStream();
            await mime.WriteToAsync(stream, ct);

            var raw = Convert.ToBase64String(stream.ToArray())
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

            var gmailMessage = new Message
            {
                Raw = raw
            };

            var sent = await service.Users.Messages.Send(gmailMessage, "me").ExecuteAsync(ct);

            return  sent.Id != null ? sent.Id : mime.MessageId ?? "";
        }

        private async Task EnsureTokenAsync(Dictionary<string, string> config, CancellationToken ct)
        {
            await _tokenService.EnsureValidAccessTokenAsync(config, ct);
        }

        private static GmailService CreateGmailService(Dictionary<string, string> config)
        {
            var accessToken = GetRequired(config, "accessToken");

            var credential = GoogleCredential.FromAccessToken(accessToken);

            return new GmailService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Marilog"
                });
        }

        private static async Task<List<string>> ListMessageIdsAsync(GmailService service, string query, CancellationToken ct)
        {
            var result = new List<string>();
            string? pageToken = null;

            do
            {
                ct.ThrowIfCancellationRequested();

                var request = service.Users.Messages.List("me");
                request.Q = query;
                request.PageToken = pageToken;
                request.MaxResults = 100;

                var response = await request.ExecuteAsync(ct);

                if (response.Messages is not null)
                {
                    result.AddRange(
                        response.Messages
                            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                            .Select(x => x.Id!));
                }

                pageToken = response.NextPageToken;

            } while (!string.IsNullOrWhiteSpace(pageToken));

            return result;
        }

        private static async Task<Message?> GetMessageAsync(GmailService service, string messageId, CancellationToken ct)
        {
            var request = service.Users.Messages.Get("me", messageId);

            request.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;

            return await request.ExecuteAsync(ct);
        }

        private async Task<InboundMessage> MapMessageAsync(GmailService service, Message message, CancellationToken ct)
        {
            var headers = message.Payload?.Headers ?? new List<MessagePartHeader>();

            var fromHeader = GetHeader(headers, "From");
            var toHeader = GetHeader(headers, "To");
            var ccHeader = GetHeader(headers, "Cc");
            var subject = GetHeader(headers, "Subject");
            var messageId = GetHeader(headers, "Message-Id");

            var fromMailbox = MailboxAddress.TryParse(fromHeader ?? string.Empty, out var parsedFrom)
                ? parsedFrom
                : new MailboxAddress(string.Empty, string.Empty);

            var inbound = new InboundMessage
            {
                ExternalId = !string.IsNullOrWhiteSpace(messageId)
                    ? messageId
                    : message.Id ?? Guid.NewGuid().ToString(),

                Subject = subject ?? string.Empty,

                ReceivedAt = message.InternalDate.HasValue
                    ? DateTimeOffset.FromUnixTimeMilliseconds(message.InternalDate.Value).UtcDateTime
                    : DateTime.UtcNow,

                FromAddress = fromMailbox.Address,
                FromDisplayName = fromMailbox.Name,

                ToAddresses = ParseAddresses(toHeader),
                CcAddresses = ParseAddresses(ccHeader)
            };

            var body = ExtractBody(message.Payload);
            inbound.Body = body.Html ?? body.Text ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(message.Id))
            {
                await ExtractAttachmentsAsync(
                    service,
                    message.Id,
                    message.Payload,
                    inbound,
                    ct);
            }

            return inbound;
        }

        private static BodyResult ExtractBody(MessagePart? part)
        {
            if (part is null)
                return new BodyResult();

            if (part.Parts is not null && part.Parts.Count > 0)
            {
                string? html = null;
                string? text = null;

                foreach (var child in part.Parts)
                {
                    var result = ExtractBody(child);

                    html ??= result.Html;
                    text ??= result.Text;

                    if (html is not null && text is not null)
                        break;
                }

                return new BodyResult
                {
                    Html = html,
                    Text = text
                };
            }

            var data = part.Body?.Data;

            if (string.IsNullOrWhiteSpace(data))
                return new BodyResult();

            var decoded = DecodeBase64Url(data);
            var mimeType = part.MimeType?.ToLowerInvariant();

            return mimeType switch
            {
                "text/html" => new BodyResult { Html = decoded },
                "text/plain" => new BodyResult { Text = decoded },
                _ => new BodyResult()
            };
        }

        private async Task ExtractAttachmentsAsync(GmailService service, string messageId, MessagePart? part, 
                                                   InboundMessage target, CancellationToken ct)
        {
            if (part is null)
                return;

            if (!string.IsNullOrWhiteSpace(part.Filename) &&
                !string.IsNullOrWhiteSpace(part.Body?.AttachmentId))
            {
                var attachment = await service.Users.Messages.Attachments.Get(
                        "me",
                        messageId,
                        part.Body.AttachmentId)
                    .ExecuteAsync(ct);

                if (!string.IsNullOrWhiteSpace(attachment.Data))
                {
                    target.Attachments.Add(
                        new InboundAttachment
                        {
                            FileName = part.Filename,
                            ContentType = part.MimeType ?? "application/octet-stream",
                            Content = DecodeBase64UrlBytes(attachment.Data)
                        });
                }
            }

            if (part.Parts is null)
                return;

            foreach (var child in part.Parts)
            {
                await ExtractAttachmentsAsync(
                    service,
                    messageId,
                    child,
                    target,
                    ct);
            }
        }

        private static MimeMessage BuildMimeMessage(string fromAddress, string? fromDisplayName, OutboundMessage message)
        {
            var mime = new MimeMessage();

            // استخدام طريقة آمنة لتوليد Message-ID عبر MimeKit
            mime.MessageId = MimeUtils.GenerateMessageId(
                fromAddress.Contains('@') ? fromAddress.Split('@')[1] : "localhost");

            mime.From.Add(
                new MailboxAddress(
                    fromDisplayName ?? fromAddress,
                    fromAddress));

            foreach (var to in message.ToAddresses)
            {
                if (MailboxAddress.TryParse(to, out var parsedTo))
                    mime.To.Add(parsedTo);
            }

            foreach (var cc in message.CcAddresses)
            {
                if (MailboxAddress.TryParse(cc, out var parsedCc))
                    mime.Cc.Add(parsedCc);
            }

            mime.Subject = message.Subject;

            var builder = new BodyBuilder
            {
                HtmlBody = message.Body
            };

            foreach (var attachment in message.Attachments)
            {
                builder.Attachments.Add(
                    attachment.FileName,
                    attachment.Content,
                    ContentType.Parse(attachment.ContentType));
            }

            mime.Body = builder.ToMessageBody();

            return mime;
        }

        private static string BuildDateQuery(DateTime since)
        {
            var unixSeconds = new DateTimeOffset(since.ToUniversalTime()).ToUnixTimeSeconds();
            return $"after:{unixSeconds}";
        }

        private static string? GetHeader(IList<MessagePartHeader> headers, string name)
        {
            return headers
                .FirstOrDefault(
                    x => string.Equals(
                        x.Name,
                        name,
                        StringComparison.OrdinalIgnoreCase))
                ?.Value;
        }

        private static List<string> ParseAddresses(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new List<string>();

            try
            {
                return InternetAddressList
                    .Parse(value)
                    .Mailboxes
                    .Select(x => x.Address)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static string DecodeBase64Url(string value)
        {
            return Encoding.UTF8.GetString(DecodeBase64UrlBytes(value));
        }

        private static byte[] DecodeBase64UrlBytes(string value)
        {
            var normalized = value
                .Replace('-', '+')
                .Replace('_', '/');

            switch (normalized.Length % 4)
            {
                case 2:
                    normalized += "==";
                    break;
                case 3:
                    normalized += "=";
                    break;
            }

            return Convert.FromBase64String(normalized);
        }

        private static string GetRequired(Dictionary<string, string> config, string key)
        {
            if (!config.TryGetValue(key, out var value) ||
                string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    $"Google configuration is missing '{key}'.");
            }

            return value;
        }

        // كلاس داخلي لتجميع نتيجة استخراج نص البريد Electron
        
    }
}