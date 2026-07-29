using Marilog.Contracts.DTOs.Requests.EmailDTOs;


namespace Marilog.Contracts.Interfaces.Services.EmailServices
{
    /// <summary>
    /// One implementation per ProviderType (Imap, MicrosoftGraph, Gmail).
    /// Takes already-decrypted config — never touches EncryptedConfig or encryption directly.
    /// </summary>
    public interface IEmailProviderClient
    {
        Task<IReadOnlyList<InboundMessage>> FetchNewMessagesAsync(
            Dictionary<string, string> config,
            DateTime since,
            CancellationToken ct = default);

        Task SendAsync(
            Dictionary<string, string> config,
            string fromAddress,
            string? fromDisplayName,
            OutboundMessage message,
            CancellationToken ct = default);
    }
}
