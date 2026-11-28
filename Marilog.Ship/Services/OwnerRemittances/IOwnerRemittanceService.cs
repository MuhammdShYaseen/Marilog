namespace Marilog.Ship.Services.OwnerRemittances
{
    /// <summary>MGA page 4 — money the owner sent to the master or paid on his behalf.</summary>
    public interface IOwnerRemittanceService
    {
        /// <summary>Dated from–to (inclusive), newest first.</summary>
        Task<IReadOnlyList<OwnerRemittanceResponse>> GetAsync(DateOnly from, DateOnly to, CancellationToken ct = default);

        Task CreateAsync(SaveOwnerRemittanceRequest request, CancellationToken ct = default);
        Task UpdateAsync(int id, SaveOwnerRemittanceRequest request, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);


        /// <summary>Wages the office paid ashore for master-paid crew; the unpaid rest stays on each balance.</summary>
        Task CreateWagesPaidAshoreAsync(SaveWagesPaidAshoreRequest request, CancellationToken ct = default);
    }
}