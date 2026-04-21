using Marilog.Kernel.Enums;


namespace Marilog.Application.DTOs.Reports.CrewContractReports
{
    public sealed class RankContractSummary
    {
        public int RankId { get; init; }
        public string RankName { get; init; } = string.Empty;
        public Department Department { get; init; }
        public int TotalContracts { get; init; }
        public int ActiveContracts { get; init; }
        public decimal AverageWage { get; init; }
    }
}