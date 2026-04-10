namespace Marilog.Contracts.DTOs.Requests.VesselDTOs
{
    public record UpdateVesselRequest(
    string VesselName,
    string? ImoNumber = null,
    decimal? GrossTonnage = null,
    int? FlagCountryId = null,
    string? Notes = null
);
}
