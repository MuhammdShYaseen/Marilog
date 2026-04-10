using Marilog.Contracts.Enums;

namespace Marilog.Contracts.DTOs.Reports.CrewContractReports
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