using Marilog.Contracts.DTOs.Requests.EmailDTOs;

namespace Marilog.Application.Interfaces.Email
{
    /// <summary>
    /// One implementation per ProviderType (Imap, MicrosoftGraph, Gmail).
    /// Takes already-decrypted config — never touches EncryptedConfig or encryption directly.
    /// </summary>
    public interface IEmailProviderClient
    {
        /// <summary>Fetches new messages from the Inbox folder.</summary>
        Task<IReadOnlyList<InboundMessage>> FetchNewMessagesAsync(
            Dictionary<string, string> config,
            DateTime since,
            CancellationToken ct = default);

        /// <summary>
        /// Fetches messages already sent from this account (Sent/Sent Items
        /// folder) — catches anything sent outside Marilog (e.g. a person
        /// replying directly from Outlook/webmail) so it still ends up logged.
        /// Reuses the InboundMessage shape since the fields are identical;
        /// the caller decides Direction based on which method it called.
        /// </summary>
        Task<IReadOnlyList<InboundMessage>> FetchSentMessagesAsync(
            Dictionary<string, string> config,
            DateTime since,
            CancellationToken ct = default);

        Task<string> SendAsync(
            Dictionary<string, string> config,
            string fromAddress,
            string? fromDisplayName,
            OutboundMessage message,
            CancellationToken ct = default);
    }
}