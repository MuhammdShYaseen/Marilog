using Marilog.Contracts.DTOs.Requests.EmailDTOs;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IEmailAccountService
    {
        Task<EmailAccountResponse?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<EmailAccountResponse>> GetAllAsync(CancellationToken ct = default);

        Task<EmailAccountResponse> CreateAsync(CreateEmailAccountRequest request, CancellationToken ct = default);

        Task RenameAsync(int id, string displayName, CancellationToken ct = default);

        Task UpdateConfigAsync(int id, Dictionary<string, string>? config, CancellationToken ct = default);

        Task ActivateAsync(int id, CancellationToken ct = default);

        Task DeactivateAsync(int id, CancellationToken ct = default);

        Task DeleteAsync(int id, CancellationToken ct = default);

        // ── Internal use only (MailWorker / IEmailProviderClient) ──────────────
        // Never expose these two through the controller — they touch secrets.

        Task<Dictionary<string, string>> GetDecryptedConfigAsync(int id, CancellationToken ct = default);

        Task MarkSyncedAsync(int id, DateTime syncedAt, CancellationToken ct = default);
    }
}





