using Marilog.Domain.Entities;

namespace Marilog.Application.Interfaces.Services
{
    public interface IPersonService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<Person?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<Person?>              GetByPassportAsync(string passportNo, CancellationToken ct = default);
        Task<Person?>              GetBySeamanBookAsync(string seamanBookNo, CancellationToken ct = default);
        Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Person>> GetActiveAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Person>> SearchAsync(string term, CancellationToken ct = default);
        Task<IReadOnlyList<Person>> GetWithExpiringPassportsAsync(int withinDays, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<Person> CreateAsync(string fullName, int? nationality = null,
                                 string? passportNo = null, DateOnly? passportExpiry = null,
                                 string? seamanBookNo = null, DateOnly? dateOfBirth = null,
                                 string? phone = null, string? email = null,
                                 CancellationToken ct = default);
        Task         UpdateAsync(int id, string fullName, int? nationality = null,
                                 string? passportNo = null, DateOnly? passportExpiry = null,
                                 string? seamanBookNo = null, DateOnly? dateOfBirth = null,
                                 string? phone = null, string? email = null,
                                 CancellationToken ct = default);
        Task         ActivateAsync(int id, CancellationToken ct = default);
        Task         DeactivateAsync(int id, CancellationToken ct = default);
        Task         DeleteAsync(int id, CancellationToken ct = default);

        // ── Bank Account ──────────────────────────────────────────────────────────
        Task UpdateBankAccountAsync(int id, string? bankName, string? iban,
                                    string? bankSwiftCode, CancellationToken ct = default);
        Task ClearBankAccountAsync(int id, CancellationToken ct = default);
    }
}
