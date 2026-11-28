namespace Marilog.Ship.Services.CrewAccount
{
    /// <summary>
    /// What a crew member took on board (MGA page 5) and the extras payable on top of wages (MGA page 2).
    /// </summary>
    public interface ICrewAccountService
    {
        // ── Withdrawals ──────────────────────────────────────────────────────────
        Task<IReadOnlyList<CrewWithdrawalResponse>> GetWithdrawalsAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
        Task CreateWithdrawalAsync(SaveCrewWithdrawalRequest request, CancellationToken ct = default);
        Task UpdateWithdrawalAsync(int id, SaveCrewWithdrawalRequest request, CancellationToken ct = default);
        Task DeleteWithdrawalAsync(int id, CancellationToken ct = default);

        // ── Extras ───────────────────────────────────────────────────────────────
        Task<IReadOnlyList<CrewExtraResponse>> GetExtrasAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
        Task CreateExtraAsync(SaveCrewExtraRequest request, CancellationToken ct = default);
        Task UpdateExtraAsync(int id, SaveCrewExtraRequest request, CancellationToken ct = default);
        Task DeleteExtraAsync(int id, CancellationToken ct = default);
    }
}