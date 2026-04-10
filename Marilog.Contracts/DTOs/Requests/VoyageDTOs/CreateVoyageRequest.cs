namespace Marilog.Contracts.DTOs.Requests.VoyageDTOs
{
    public record CreateVoyageRequest(
    int VesselId,
    string VoyageNumber,
    DateOnly VoyageMonth,
    int? MasterContractId = null,
    int? DeparturePortId = null,
    int? ArrivalPortId = null,
    DateTime? DepartureDate = null,
    DateTime? ArrivalDate = null,
    string? CargoType = null,
    decimal? CargoQuantityMt = null,
    string? Notes = null
);
}
