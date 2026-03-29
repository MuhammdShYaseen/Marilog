namespace Marilog.Presentation.DTOs.VoyageDTOs
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
