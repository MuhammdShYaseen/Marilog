namespace Marilog.Contracts.DTOs.Requests.VoyageDTOs
{
    public record UpdateVoyageRequest(
        DateTime? StartDate,
        DateTime? EndDate,
        string? Notes
    );
}