namespace Marilog.Contracts.DTOs.Requests.VoyageDTOs
{
    public record UpdateVoyageRequest(
    int? DeparturePortId,
    int? ArrivalPortId,
    DateTime? DepartureDate,
    DateTime? ArrivalDate,
    string? CargoType,
    decimal? CargoQuantityMt,
    string? Notes
);
}
