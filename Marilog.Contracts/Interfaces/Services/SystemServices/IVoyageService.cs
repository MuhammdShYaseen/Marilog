using Marilog.Contracts.DTOs.Reports.VoyageReports;
using Marilog.Contracts.DTOs.Requests.VoyageDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;


namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IVoyageService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<VoyageResponse?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<VoyageLookupResponse>> GetAllLookupAsync(CancellationToken ct = default);
        Task<IReadOnlyList<VoyageResponse>>GetActiveVoyagesAsync(CancellationToken ct = default);
        Task<VoyageResponse?>              GetWithStopsAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<VoyageResponse>> GetByVesselAsync(int vesselId, CancellationToken ct = default);
        Task<IReadOnlyList<VoyageResponse>> GetByMonthAsync(DateOnly month, CancellationToken ct = default);
        Task<IReadOnlyList<VoyageResponse>> GetByStatusAsync(VoyageStatus status, CancellationToken ct = default);
        Task<VoyageResponse?>              GetCurrentVoyageAsync(int vesselId, CancellationToken ct = default);
        //---Reports----------------------------------------------------------------
        Task<VoyageReport> GetVoyagesReportAsync(VoyageReportFilterOptions options, CancellationToken ct = default);
        // ── Commands ─────────────────────────────────────────────────────────────
        Task<VoyageResponse> CreateAsync(CreateVoyageRequest request, CancellationToken ct = default);
        Task<IReadOnlyList<VoyageResponse>> CreateRangeAsync(IEnumerable<CreateVoyageRequest> commands, CancellationToken ct = default);
        Task UpdateAsync(int id, UpdateVoyageRequest request, CancellationToken ct = default);
        Task         AssignMasterAsync(int id, int contractId, CancellationToken ct = default);
        Task         StartAsync(int id, CancellationToken ct = default);
        Task         CompleteAsync(int id, CancellationToken ct = default);
        Task         CancelAsync(int id, CancellationToken ct = default);
        Task         DeleteAsync(int id, CancellationToken ct = default);

        // ── Stops ─────────────────────────────────────────────────────────────────
        Task<VoyageStopResponse> AddStopAsync(int voyageId, AddVoyageStopRequest request, CancellationToken ct = default);
        Task UpdateStopAsync(int voyageId, int stopOrder, UpdateVoyageStopRequest request, CancellationToken ct = default);
        Task             RemoveStopAsync(int voyageId, int stopOrder, CancellationToken ct = default);



        // ── Contracts ─────────────────────────────────────────────────────────────
        Task LinkContractAsync(int voyageId, int contractId, CancellationToken ct = default);
        Task UnlinkContractAsync(int voyageId, int contractId, CancellationToken ct = default);

        // ── Cargo lines ───────────────────────────────────────────────────────────
        Task AddCargoAsync(int voyageId, int stopOrder, SaveVoyageCargoRequest request, CancellationToken ct = default);
        Task UpdateCargoAsync(int voyageId, int voyageCargoId, SaveVoyageCargoRequest request, CancellationToken ct = default);
        Task RemoveCargoAsync(int voyageId, int voyageCargoId, CancellationToken ct = default);
    }
}
