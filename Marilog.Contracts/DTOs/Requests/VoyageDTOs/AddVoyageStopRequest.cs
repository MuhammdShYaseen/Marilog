namespace Marilog.Contracts.DTOs.Requests.VoyageDTOs
{
    public record AddVoyageStopRequest(
    int PortId,
    int StopOrder,
    DateTime? ArrivalDate = null,
    DateTime? DepartureDate = null,
    string? PurposeOfCall = null,
    string? Notes = null
);
}
