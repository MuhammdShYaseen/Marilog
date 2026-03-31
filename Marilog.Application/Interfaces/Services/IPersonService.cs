using Marilog.Application.DTOs.Commands.Person;
using Marilog.Application.DTOs.Responses;
using Marilog.Domain.Entities;

namespace Marilog.Application.Interfaces.Services
{
    public interface IPersonService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<PersonResponse?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<PersonResponse?>              GetByPassportAsync(string passportNo, CancellationToken ct = default);
        Task<PersonResponse?>              GetBySeamanBookAsync(string seamanBookNo, CancellationToken ct = default);
        Task<IReadOnlyList<PersonResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<PersonResponse>> GetActiveAsync(CancellationToken ct = default);
        Task<IReadOnlyList<PersonResponse>> SearchAsync(string term, CancellationToken ct = default);
        Task<IReadOnlyList<PersonResponse>> GetWithExpiringPassportsAsync(int withinDays, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<PersonResponse> CreateAsync(string bankName, string iBAN, bool isPassportExpired, string? bankSwiftCode, string fullName, int? nationality = null,
            string? passportNo = null, DateOnly? passportExpiry = null,
            string? seamanBookNo = null, DateOnly? dateOfBirth = null,
            string? phone = null, string? email = null,
            CancellationToken ct = default);

        Task<IReadOnlyList<PersonResponse>> CreateRangeAsync(IEnumerable<CreatePersonCommand> commands, CancellationToken ct = default);
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
