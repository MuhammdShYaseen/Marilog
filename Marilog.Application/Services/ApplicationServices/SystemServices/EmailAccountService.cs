
using Marilog.Application.Interfaces.Encryption;
using Marilog.Contracts.DTOs.Requests.EmailDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Json;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class EmailAccountService : IEmailAccountService
    {
        private readonly IRepository<EmailAccount> _repo;
        private readonly ISecretEncryptionService _encryption; // assumed — same AES service used by AiProviders

        public EmailAccountService(IRepository<EmailAccount> repo, ISecretEncryptionService encryption)
        {
            _repo = repo;
            _encryption = encryption;
        }

        // ── Mapping ───────────────────────────────────────────────────────────────

        private static readonly Expression<Func<EmailAccount, EmailAccountResponse>> ToResponse = account => new EmailAccountResponse
        {
            Id = account.Id,
            DisplayName = account.DisplayName,
            EmailAddress = account.EmailAddress,
            ProviderType = account.ProviderType,
            IsActive = account.IsActive,
            LastSyncedAt = account.LastSyncedAt
        };

        private static readonly Func<EmailAccount, EmailAccountResponse> ToResponseCompiled = ToResponse.Compile();

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<EmailAccountResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<EmailAccountResponse>> GetAllAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .OrderBy(x => x.DisplayName)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<EmailAccountResponse> CreateAsync(CreateEmailAccountRequest request, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(request.Config);
            var encryptedConfig = _encryption.Encrypt(json);

            var account = EmailAccount.Create(
                request.DisplayName,
                request.EmailAddress,
                request.ProviderType,
                encryptedConfig);

            await _repo.AddAsync(account, ct);
            await _repo.SaveChangesAsync(ct);

            return ToResponseCompiled(account);
        }

        public async Task RenameAsync(int id, string displayName, CancellationToken ct = default)
        {
            var account = await GetOrThrowAsync(id, ct);
            account.Rename(displayName);
            _repo.Update(account);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task UpdateConfigAsync(int id, Dictionary<string, string>? config, CancellationToken ct = default)
        {

            if(config == null)
            {
                throw new ArgumentNullException(nameof(config) + " cannt be null");
            }
            var account = await GetOrThrowAsync(id, ct);

            var json = JsonSerializer.Serialize(config);
            var encryptedConfig = _encryption.Encrypt(json);

            account.UpdateConfig(encryptedConfig);
            _repo.Update(account);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var account = await GetOrThrowAsync(id, ct);
            account.Activate();
            _repo.Update(account);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var account = await GetOrThrowAsync(id, ct);
            account.Deactivate();
            _repo.Update(account);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var account = await GetOrThrowAsync(id, ct);
            _repo.HardDelete(account);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Internal use only ────────────────────────────────────────────────────

        public async Task<Dictionary<string, string>> GetDecryptedConfigAsync(int id, CancellationToken ct = default)
        {
            var account = await GetOrThrowAsync(id, ct);

            var json = _encryption.Decrypt(account.EncryptedConfig);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                   ?? new Dictionary<string, string>();
        }

        public async Task MarkSyncedAsync(int id, DateTime syncedAt, CancellationToken ct = default)
        {
            var account = await GetOrThrowAsync(id, ct);
            account.MarkSynced(syncedAt);
            _repo.Update(account);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<EmailAccount> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"EmailAccount {id} not found.");
    }
}