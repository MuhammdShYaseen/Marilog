using Marilog.Kernel.Enums;

namespace Marilog.Ship.Services.Setup
{
    public interface IShipSetupService
    {
        Task<ShipSetupStatusResponse> GetStatusAsync(CancellationToken ct = default);

        /// <summary>Step 1+2: owner company, vessel, base currency, default WagesPaidBy.</summary>
        Task SaveVesselAsync(SaveShipVesselRequest request, CancellationToken ct = default);

        /// <summary>Crew contracts are created with CrewContractService; this sets Master/Office on one.</summary>
        Task SetContractWagesPaidByAsync(int crewContractId, WagesPaidBy wagesPaidBy, CancellationToken ct = default);

        Task<OpeningBalancesResponse?> GetOpeningAsync(CancellationToken ct = default);
        Task SaveOpeningAsync(SaveOpeningBalancesRequest request, CancellationToken ct = default);
        Task<OpeningCheckResponse> CheckOpeningAsync(CancellationToken ct = default);

        /// <summary>Locks the setup and the opening balances. A non-zero difference needs a reason.</summary>
        Task CompleteAsync(string? differenceReason, CancellationToken ct = default);
    }
}