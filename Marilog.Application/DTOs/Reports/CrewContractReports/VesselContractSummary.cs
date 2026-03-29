namespace Marilog.Application.DTOs.Reports.CrewContractReports
{
    public sealed class VesselContractSummary
    {
        public int VesselId { get; init; }
        public string VesselName { get; init; } = string.Empty;
        public int TotalContracts { get; init; }
        public int ActiveContracts { get; init; }
        public decimal AverageWage { get; init; }
    }
}