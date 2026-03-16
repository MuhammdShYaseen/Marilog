using Marilog.Domain.Entities;

namespace Marilog.Domain.Interfaces.Services
{
    public interface ICrewBankAccountService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<CrewBankAccount?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<CrewBankAccount>> GetByPersonAsync(int personId, CancellationToken ct = default);
        Task<CrewBankAccount?>              GetDefaultAsync(int personId, int currencyId, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<CrewBankAccount> CreateAsync(int personId, int currencyId, string bankName,
                                          string accountNumber, string? iban = null,
                                          string? swiftCode = null, string? bankAddress = null,
                                          bool isDefault = false, CancellationToken ct = default);
        Task                  UpdateAsync(int id, string bankName, string accountNumber,
                                          string? iban = null, string? swiftCode = null,
                                          string? bankAddress = null, CancellationToken ct = default);
        Task                  SetAsDefaultAsync(int id, CancellationToken ct = default);
        Task                  ActivateAsync(int id, CancellationToken ct = default);
        Task                  DeactivateAsync(int id, CancellationToken ct = default);
        Task                  DeleteAsync(int id, CancellationToken ct = default);
    }
}
