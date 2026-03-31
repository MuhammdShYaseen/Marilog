
namespace Marilog.Application.DTOs.Commands.Vessel
{
    public record CreateVesselCommand(
    int CompanyId,
    string VesselName,
    string? ImoNumber = null,
    decimal? GrossTonnage = null,
    int? FlagCountryId = null,
    string? Notes = null
);
}
