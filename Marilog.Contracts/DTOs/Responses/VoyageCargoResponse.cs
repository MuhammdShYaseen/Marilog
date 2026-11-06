using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{
    public class VoyageCargoResponse
    {
        public int VoyageCargoId { get; set; }
        public int CargoTypeId { get; set; }
        public string CargoTypeName { get; set; } = null!;
        public CargoCategory Category { get; set; }
        public CargoOperation Operation { get; set; }
        public decimal Quantity { get; set; }
        public CargoUnit Unit { get; set; }
        public decimal? WeightMt { get; set; }
        public int? ContractId { get; set; }
        public string? ContractNumber { get; set; }
        public string? Notes { get; set; }

        // Dry bulk extension
        public decimal? DraftSurveyMt { get; set; }
        public decimal? ShoreFigureMt { get; set; }

        // General cargo extension
        public int? PackageCount { get; set; }
        public string? PackageType { get; set; }
        public decimal? VolumeCbm { get; set; }
    }
}