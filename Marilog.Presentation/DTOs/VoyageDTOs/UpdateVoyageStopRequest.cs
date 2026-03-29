namespace Marilog.Presentation.DTOs.VoyageDTOs
{
    public record UpdateVoyageStopRequest(
    DateTime? ArrivalDate,
    DateTime? DepartureDate,
    string? PurposeOfCall,
    string? Notes
);
}
