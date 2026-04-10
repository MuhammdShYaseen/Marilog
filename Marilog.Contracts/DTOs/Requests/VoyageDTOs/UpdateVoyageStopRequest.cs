namespace Marilog.Contracts.DTOs.Requests.VoyageDTOs
{
    public record UpdateVoyageStopRequest(
    DateTime? ArrivalDate,
    DateTime? DepartureDate,
    string? PurposeOfCall,
    string? Notes
);
}
