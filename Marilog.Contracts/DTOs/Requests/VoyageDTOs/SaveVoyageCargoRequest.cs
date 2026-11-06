using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.VoyageDTOs
{
    public record SaveVoyageCargoRequest(
        int CargoTypeId,
        CargoOperation Operation,
        decimal Quantity,
        CargoUnit Unit,
        decimal? WeightMt = null,
        int? ContractId = null,
        string? Notes = null,
        // Dry bulk extension
        decimal? DraftSurveyMt = null,
        decimal? ShoreFigureMt = null,
        // General cargo extension
        int? PackageCount = null,
        string? PackageType = null,
        decimal? VolumeCbm = null
    );
}