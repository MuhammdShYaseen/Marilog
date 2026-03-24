using Marilog.Domain.Entities;

namespace Marilog.Application.Interfaces.Services
{
    public interface IVoyageService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<Voyage?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<Voyage?>              GetWithStopsAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<Voyage>> GetByVesselAsync(int vesselId, CancellationToken ct = default);
        Task<IReadOnlyList<Voyage>> GetByMonthAsync(DateOnly month, CancellationToken ct = default);
        Task<IReadOnlyList<Voyage>> GetByStatusAsync(VoyageStatus status, CancellationToken ct = default);
        Task<Voyage?>              GetCurrentVoyageAsync(int vesselId, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<Voyage> CreateAsync(int vesselId, string voyageNumber, DateOnly voyageMonth,
                                 int? masterContractId = null, int? departurePortId = null,
                                 int? arrivalPortId = null, DateTime? departureDate = null,
                                 DateTime? arrivalDate = null, string? cargoType = null,
                                 decimal? cargoQuantityMt = null, string? notes = null,
                                 CancellationToken ct = default);
        Task         UpdateAsync(int id, int? departurePortId, int? arrivalPortId,
                                 DateTime? departureDate, DateTime? arrivalDate,
                                 string? cargoType, decimal? cargoQuantityMt,
                                 string? notes, CancellationToken ct = default);
        Task         UpdateFinancialsAsync(int id, decimal cashOnBoard,
                                           decimal cigarettesOnBoard,
                                           decimal previousMasterBalance,
                                           CancellationToken ct = default);
        Task         AssignMasterAsync(int id, int contractId, CancellationToken ct = default);
        Task         StartAsync(int id, CancellationToken ct = default);
        Task         CompleteAsync(int id, CancellationToken ct = default);
        Task         CancelAsync(int id, CancellationToken ct = default);
        Task         DeleteAsync(int id, CancellationToken ct = default);

        // ── Stops ─────────────────────────────────────────────────────────────────
        Task<VoyageStop> AddStopAsync(int voyageId, int portId, int stopOrder,
                                      DateTime? arrivalDate = null, DateTime? departureDate = null,
                                      string? purposeOfCall = null, string? notes = null,
                                      CancellationToken ct = default);
        Task             UpdateStopAsync(int voyageId, int stopOrder, DateTime? arrivalDate,
                                         DateTime? departureDate, string? purposeOfCall,
                                         string? notes, CancellationToken ct = default);
        Task             RemoveStopAsync(int voyageId, int stopOrder, CancellationToken ct = default);
    }
}
